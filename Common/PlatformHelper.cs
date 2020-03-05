using System;

namespace Common
{
    public static class PlatformHelper
    {
        public static bool OsIsUnix()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix;
        }
    }
}