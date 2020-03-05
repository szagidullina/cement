using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.NuGet;
using Microsoft.Extensions.Logging;
using NuGet.CommandLine;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using PackageDownloader = NuGet.PackageManagement.PackageDownloader;
using PackageSourceProvider = NuGet.Configuration.PackageSourceProvider;
using Settings = NuGet.Configuration.Settings;

namespace Common
{
    public class NuGetPackageHepler
    {
        private readonly ILogger log;

        public NuGetPackageHepler(ILogger log)
        {
            this.log = log;
        }

        private class NuGetProject
        {
            private readonly List<string> packagesList;
            private readonly ProjectFile projectFile;
            private readonly MSBuildNuGetProject project;
            private readonly ConsoleProjectContext projectContext;
            private readonly MSBuildProjectSystem projectSystem;
            private readonly List<SourceRepository> repositories;
            private readonly HashSet<PackageIdentity> installedPackages;
            private readonly ILogger log;

            public NuGetProject(List<string> packagesList, string packagesPath, ProjectFile projectFile, ILogger log)
            {
                this.log = log;
                this.packagesList = packagesList;
                this.projectFile = projectFile;
                installedPackages = new HashSet<PackageIdentity>();
                var sourceProvider = new PackageSourceProvider(Settings.LoadDefaultSettings(null));
                var sourceRepositoryProvider = new CommandLineSourceRepositoryProvider(sourceProvider);
                repositories = sourceProvider.LoadPackageSources().Select(sourceRepositoryProvider.CreateRepository)
                    .ToList();

                var projectFilePath = projectFile.FilePath;

                var msbuildDirectory =
                    Path.GetDirectoryName(ModuleBuilderHelper.FindMsBuild(null, "Cement NuGet Package Installer").Path);
                projectContext = new ConsoleProjectContext(new MicrosoftNuGetLoggerAdapter(log));
                projectSystem = new MSBuildProjectSystem(
                    msbuildDirectory,
                    projectFilePath,
                    projectContext);
                var projectFolder = Path.GetDirectoryName(projectFilePath);
                project = new MSBuildNuGetProject(projectSystem, packagesPath, projectFolder);
            }

            public async Task InstallAsync()
            {
                using (var sourceCacheContext = new SourceCacheContext())
                {
                    var packageDownloadContext = new PackageDownloadContext(sourceCacheContext);
                    foreach (var packageName in packagesList)
                    {
                        var package = ParsePackage(packageName);
                        await InstallPackageWithDependenciesAsync(package, packageDownloadContext);
                    }
                }

                projectSystem.Save();

                var projectFileContent = File.ReadAllText(projectSystem.ProjectFileFullPath);
                var contentLines = projectFileContent
                    .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                contentLines[0] = contentLines[0].Replace("utf-16", "utf-8");
                File.WriteAllText(
                    projectSystem.ProjectFileFullPath,
                    string.Join(projectFile.LineEndings, contentLines),
                    new UTF8Encoding(true));
            }

            private async Task InstallPackageWithDependenciesAsync(PackageIdentity package,
                PackageDownloadContext packageDownloadContext)
            {
                log.LogInformation($"Loading package {package}");
                var downloadResourceResult = await LoadPackageAsync(package, packageDownloadContext);
                var dependencyGroups = downloadResourceResult.PackageReader.GetPackageDependencies().ToList();
                var mostCompatibleFramework = new FrameworkReducer().GetNearest(
                    projectSystem.TargetFramework,
                    dependencyGroups.Select(dg => dg.TargetFramework));
                var dependencyGroup = dependencyGroups.FirstOrDefault(ds =>
                    ds.TargetFramework.Equals(mostCompatibleFramework));
                if (dependencyGroup != null)
                {
                    foreach (var dependency in dependencyGroup.Packages)
                    {
                        var dependencyIdentity = new PackageIdentity(dependency.Id,
                            NuGetVersion.Parse(dependency.VersionRange.MinVersion.ToFullString()));
                        log.LogInformation($"Resolved dependency of {package}: {dependencyIdentity}");
                        if (installedPackages.Contains(dependencyIdentity)) continue;
                        await InstallPackageWithDependenciesAsync(dependencyIdentity, packageDownloadContext);
                        installedPackages.Add(dependencyIdentity);
                    }
                }

                var packageIdentity = new PackageIdentity(package.Id, new NuGetVersion(package.Version.Version));
                var installSuccess = await project
                    .InstallPackageAsync(packageIdentity, downloadResourceResult, projectContext,
                        CancellationToken.None);

                if (installSuccess)
                {
                    log.LogInformation($"Installed {package}");
                }
                else
                {
                    log.LogInformation($"{package} not installed");
                    ConsoleWriter.WriteWarning($"Nuget package {package} not installed");
                }
            }

            private async Task<DownloadResourceResult> LoadPackageAsync(PackageIdentity package,
                PackageDownloadContext packageDownloadContext)
            {
                var downloadResourceResult = await PackageDownloader.GetDownloadResourceResultAsync(
                    repositories,
                    package,
                    packageDownloadContext,
                    SettingsUtility.GetGlobalPackagesFolder(Settings.LoadDefaultSettings(null)),
                    new MicrosoftNuGetLoggerAdapter(log),
                    CancellationToken.None
                );
                return downloadResourceResult;
            }

            private static PackageIdentity ParsePackage(string packageName)
            {
                var splitted = packageName.Split('/');
                if (splitted.Length != 2)
                    throw new BadNuGetPackageException(packageName);
                var packageId = splitted[0];
                var version = NuGetVersion.Parse(splitted[1]);
                return new PackageIdentity(packageId, version);
            }
        }

        public async Task InstallPackagesAsync(List<string> packagesList, string packagesPath,
            ProjectFile projectFilePath)
        {
            await new NuGetProject(packagesList, packagesPath, projectFilePath, log).InstallAsync();
        }
    }
}