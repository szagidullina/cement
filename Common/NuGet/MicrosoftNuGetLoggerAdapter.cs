using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using ILogger = NuGet.Common.ILogger;
using LogLevel = NuGet.Common.LogLevel;

namespace Common.NuGet
{
    public sealed class MicrosoftNuGetLoggerAdapter : ILogger
    {
        private readonly Microsoft.Extensions.Logging.ILogger logger;

        public MicrosoftNuGetLoggerAdapter(Microsoft.Extensions.Logging.ILogger logger)
        {
            this.logger = logger;
        }

        public void LogDebug(string data)
        {
            logger.LogDebug(data);
        }

        public void LogVerbose(string data)
        {
            logger.LogTrace(data);
        }

        public void LogInformation(string data)
        {
            logger.LogInformation(data);
        }

        public void LogMinimal(string data)
        {
            logger.LogDebug(data);
        }

        public void LogWarning(string data)
        {
            logger.LogWarning(data);
        }

        public void LogError(string data)
        {
            logger.LogWarning(data);
        }

        public void LogInformationSummary(string data)
        {
            logger.LogWarning(data);
        }

        public void Log(LogLevel level, string data)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    logger.LogDebug(data);
                    return;
                case LogLevel.Verbose:
                    logger.LogTrace(data);
                    return;
                case LogLevel.Information:
                    logger.LogInformation(data);
                    return;
                case LogLevel.Minimal:
                    logger.LogDebug(data);
                    return;
                case LogLevel.Warning:
                    logger.LogWarning(data);
                    return;
                case LogLevel.Error:
                    logger.LogError(data);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        public Task LogAsync(LogLevel level, string data)
        {
            Log(level, data);
            return Task.CompletedTask;
        }

        public void Log(ILogMessage message)
        {
            Log(message.Level, message.Message);
        }

        public Task LogAsync(ILogMessage message)
        {
            Log(message.Level, message.Message);
            return Task.CompletedTask;
        }
    }
}