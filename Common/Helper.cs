using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace Common
{
    public static class Helper
    {
        public const string ConfigurationDelimiter = "/";
        public static readonly int MaxDegreeOfParallelism;
        public static ParallelOptions ParallelOptions => new ParallelOptions {MaxDegreeOfParallelism = MaxDegreeOfParallelism};
        public static string CurrentWorkspace { get; private set; }
        public static readonly object LockObject = new object();
        public static readonly object PackageLockObject = new object();
        private static readonly ILogger Log;

        static Helper()
        {
            Log = LogManager.GetLogger(typeof(Helper));
            MaxDegreeOfParallelism = CementSettings.Get().MaxDegreeOfParallelism ?? 2 * Environment.ProcessorCount;
        }

        public static void SetWorkspace(string workspace)
        {
            CurrentWorkspace = workspace;
        }

        public static bool IsCementTrackedDirectory(string path)
        {
            return Directory.Exists(Path.Combine(path, DirectoryHelper.CementDirectory));
        }

        public static bool IsCurrentDirectoryModule(string cwd)
        {
            if (cwd.Equals(Directory.GetDirectoryRoot(cwd)))
                return false;

            if (IsCementTrackedDirectory(cwd))
                return false;

            var parentDirectory = Directory.GetParent(cwd).FullName;
            if (!IsCementTrackedDirectory(parentDirectory))
                return false;
            return true;
        }

        public static bool DirectoryContainsModule(string directory, string moduleName)
        {
            return Directory.EnumerateDirectories(directory)
                .Select(Path.GetFileName)
                .Contains(moduleName);
        }

        public static IList<Package> GetPackages()
        {
            return CementSettings.Get().Packages ?? throw new CementException("Packages not specified.");
        }

        public static IList<Module> GetModulesFromPackage(Package package)
        {
            lock (PackageLockObject)
            {
                var packageConfig = DirectoryHelper.GetPackagePath(package.Name);
                if (!File.Exists(packageConfig))
                    PackageUpdater.UpdatePackages();
                var configData = File.ReadAllText(packageConfig);
                return ModuleIniParser.Parse(configData).ToList();
            }
        }

        public static List<Module> GetModules()
        {
            lock (PackageLockObject)
            {
                var modules = new List<Module>();
                var packages = GetPackages();
                foreach (var package in packages)
                    modules.AddRange(GetModulesFromPackage(package));
                return modules;
            }
        }

        public static string TryFixModuleCase(string module)
        {
            var modules = GetModules();
            foreach (var m in modules)
                if (m.Name.ToLower() == module.ToLower())
                    return m.Name;
            return module;
        }

        public static bool HasModule(string module)
        {
            return GetModules().Any(m => m.Name == module);
        }

        public static string DefineForce(string force, GitRepository rootRepo)
        {
            if (force == null || !force.Contains("->") && !force.Contains("CURRENT_BRANCH"))
                return force;
            if (force.Equals("%CURRENT_BRANCH%") || force.Equals("$CURRENT_BRANCH"))
                return rootRepo.CurrentLocalTreeish().Value;

            return null;
        }

        public static string DefineForce(string force, string branch)
        {
            if (force == null || !force.Contains("->") && !force.Contains("CURRENT_BRANCH"))
                return force;
            if (force.Equals("%CURRENT_BRANCH%") || force.Equals("$CURRENT_BRANCH"))
                return branch;

            return null;
        }

        public static string GetCurrentBuildCommitHash()
        {
            var gitInfo = GetAssemblyTitle();
            var commitHash = gitInfo.Split('\n').Skip(1).First().Replace("Commit: ", String.Empty).Trim();
            return commitHash;
        }

        public static string GetAssemblyTitle()
        {
            return ((AssemblyTitleAttribute)
                Attribute.GetCustomAttribute(
                    Assembly.GetEntryAssembly(),
                    typeof(AssemblyTitleAttribute))).Title;
        }

        public static string ConvertTime(long millisecs)
        {
            var ts = TimeSpan.FromMilliseconds(millisecs);
            var res = ts.ToString(@"d\:hh\:mm\:ss\.fff");

            int idx = 0;
            while (res[idx] == '0' || res[idx] == ':')
            {
                idx++;
            }

            res = res.Substring(idx);
            return res;
        }

        

        public static string GetModuleDirectory(string path)
        {
            if (path == null)
                return null;
            var parent = Directory.GetParent(path);
            while (parent != null)
            {
                if (IsCementTrackedDirectory(parent.FullName))
                    return path;
                path = parent.FullName;
                parent = Directory.GetParent(parent.FullName);
            }

            return null;
        }

        public static string GetWorkspaceDirectory(string path)
        {
            var folder = new DirectoryInfo(path);
            while (folder != null && !IsCementTrackedDirectory(folder.FullName))
            {
                folder = Directory.GetParent(folder.FullName);
            }

            return folder?.FullName;
        }

        private static string GetLastUpdateFilePath()
        {
            return Path.Combine(DirectoryHelper.GetGlobalCementDirectory(), "last-update2");
        }

        public static DateTime GetLastUpdateTime()
        {
            var file = GetLastUpdateFilePath();
            if (!File.Exists(file))
                return DateTime.MinValue;

            return File.GetLastWriteTime(file);
        }

        public static void SaveLastUpdateTime()
        {
            var file = GetLastUpdateFilePath();
            DirectoryHelper.CreateFileAndDirectory(file, "");
        }

        public static void RemoveOldKey(ref string[] args, string oldKey, ILogger log)
        {
            if (args.Contains(oldKey))
            {
                ConsoleWriter.WriteError("Don't use old " + oldKey + " key.");
                log.LogWarning("Found old key " + oldKey + " in " + string.Join(" ", args) + " in " + Directory.GetCurrentDirectory());
                args = args.Where(a => a != oldKey).ToArray();
            }
        }

        public static string Encrypt(string password)
        {
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);

            byte[] cipherBytes = ProtectedData.Protect(passwordBytes, null, DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(cipherBytes);
        }

        public static string Decrypt(string cipher)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipher);

            byte[] passwordBytes = ProtectedData.Unprotect(cipherBytes, null, DataProtectionScope.CurrentUser);

            return Encoding.Unicode.GetString(passwordBytes);
        }

        public static string FixLineEndings(string text)
        {
            return text.Replace("\r\n", "\n");
        }
    }
}