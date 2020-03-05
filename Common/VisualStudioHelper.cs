using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Common
{
    public static class VisualStudioHelper
    {
        public static IReadOnlyList<string> VisualStudioEditions { get; } =
            new List<string>
            {
                "Community",
                "Professional",
                "Enterprise",
                "BuildTools",
            }.AsReadOnly();
        public static IReadOnlyList<string> VisualStudioVersions { get; } =
            new List<string>
            {
                "2017",
                "2019",
            }.AsReadOnly();

        public static string GetEnvVariableByVisualStudioVersion(string version)
        {
            switch (version)
            {
                case "2019": return "VS160COMNTOOLS";
                default: return "VS150COMNTOOLS";
            }
        }

        public static bool IsVisualStudioVersion(string version)
        {
            return !String.IsNullOrEmpty(version) && Regex.IsMatch(version, "^[0-9][0-9].[0-9]$");
        }

        public static string GetMsBuildVersion(string fullPathToMsBuild)
        {
            if (!File.Exists(fullPathToMsBuild))
                return null;

            try
            {
                var shellRunner = new ShellRunner();
                var exitCode = shellRunner.RunOnce(Path.GetFileName(fullPathToMsBuild) + " -version", Path.GetDirectoryName(fullPathToMsBuild), TimeSpan.FromSeconds(10));
                if (exitCode == 0 && !String.IsNullOrEmpty(shellRunner.Output))
                {
                    var versionMatches = Regex.Matches(shellRunner.Output, @"^(?<version>\d+(\.\d+)+)", RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                    if (versionMatches.Count > 0)
                    {
                        var version = versionMatches[versionMatches.Count - 1].Groups["version"].Value;
                        if (!String.IsNullOrEmpty(version))
                            return version;
                    }
                }
                else
                    LoggerExtensions.LogDebug(Helper.Log, "Failed to get msbuild version for " + fullPathToMsBuild);
            }
            catch (Exception e)
            {
                LoggerExtensions.LogWarning((ILogger)Helper.Log, "Failed to get MSBuild version from " + fullPathToMsBuild, e);
            }

            return null;
        }
    }
}