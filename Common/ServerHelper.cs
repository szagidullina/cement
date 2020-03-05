using System.IO;

namespace Common
{
    public static class ServerHelper
    {
        // ReSharper disable once UnusedMember.Global
        public static string GetBinariesPath()
        {
            return Path.Combine(Directory.GetDirectoryRoot(DirectoryHelper.HomeDirectory()), "CementServer", "Binaries");
        }

        public static string GetServerRepositoriesPath()
        {
            return Path.Combine(Directory.GetDirectoryRoot(DirectoryHelper.HomeDirectory()), "CementServer", "Repositories");
        }
    }
}