using System;
using System.IO;
using JetBrains.Annotations;

namespace Common
{
    public static class DirectoryHelper
    {
        public const string CementDirectory = ".cement";
        public const string YamlSpecFile = "module.yaml";

        public static string HomeDirectory()
        {
            return PlatformHelper.OsIsUnix()
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.GetEnvironmentVariable("USERPROFILE");
        }

        public static string GetGlobalCementDirectory()
        {
            return Path.Combine(DirectoryHelper.HomeDirectory(), DirectoryHelper.CementDirectory);
        }

        public static string GetCementInstallDirectory()
        {
            return Path.Combine(DirectoryHelper.HomeDirectory(), "bin");
        }

        public static string GetPackagePath(string packageName)
        {
            return Path.Combine(DirectoryHelper.GetGlobalCementDirectory(), packageName + ".cmpkg");
        }

        public static void CreateFileAndDirectory(string filePath, string content)
        {
            CreateFileAndDirectory(filePath);
            File.WriteAllText(filePath, content);
        }

        private static void CreateFileAndDirectory(string filePath)
        {
            var dir = Directory.GetParent(filePath).FullName;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (!File.Exists(filePath))
                File.Create(filePath).Close();
        }

        public static string ProgramFiles()
        {
            var programFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)") ??
                               Environment.GetEnvironmentVariable("ProgramFiles");
            return programFiles;
        }

        public static string FixPath([NotNull] string path)
        {
            return path.Replace('\\', Path.DirectorySeparatorChar);
        }

        public static string UnixPathSlashesToWindows(string path)
        {
            return path.Replace('/', '\\');
        }

        public static string WindowsPathSlashesToUnix(string path)
        {
            return path.Replace('\\', '/');
        }

        public static string GetRootFolder(string path)
        {
            while (true)
            {
                var temp = Path.GetDirectoryName(path);
                if (String.IsNullOrEmpty(temp))
                    break;
                path = temp;
            }

            return path;
        }

        public static string GetRelativePath(string filePath, string fromFolder)
        {
            var pathUri = new Uri(filePath);
            if (!fromFolder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                fromFolder += Path.DirectorySeparatorChar;
            }

            var folderUri = new Uri(fromFolder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}