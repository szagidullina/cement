using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Common
{
    public sealed class CementFromLocalPathUpdater : ICementUpdater
    {
        private readonly ILogger log;

        public CementFromLocalPathUpdater(ILogger log)
        {
            this.log = log;
        }

        public string GetNewCommitHash()
        {
            return DateTime.Now.Ticks.ToString();
        }

        public byte[] GetNewCementZip()
        {
            try
            {

                using (var fsSource = new FileStream(
                           Path.Combine(Helper.GetZipCementDirectory(), "cement.zip"),
                           FileMode.Open,
                           FileAccess.Read))
                {
                    var zipContent = new byte[fsSource.Length];
                    int numBytesToRead = (int)fsSource.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {
                        var byteBlock = fsSource.Read(zipContent, numBytesRead, numBytesToRead);

                        if (byteBlock == 0)
                            break;

                        numBytesRead += byteBlock;
                        numBytesToRead -= byteBlock;
                    }

                    return zipContent;
                }
            }
            catch (Exception ex)
            {
                log.LogError("Fail self-update, exception: '{Message}' ", ex.Message);
            }

            return null;
        }

        public string GetName() =>
            "fileSystemLocalPath";
    }
}