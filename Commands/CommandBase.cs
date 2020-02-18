using System;
using System.Diagnostics;
using System.IO;
using Common;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace Commands
{
    public abstract class CommandBase : ICommand
    {
        protected ILogger Log { get; private set; }
        protected CommandSettings CommandSettings { get; }

        protected CommandBase(CommandSettings settings)
        {
            CommandSettings = settings;
        }

        public int Run(string[] args)
        {
            try
            {
                var sw = Stopwatch.StartNew();

                SetWorkspace();
                CheckRequireYaml();
                InitLogging();
                LogAndParseArgs(args);

                var exitCode = Execute();

                if (!CommandSettings.NoElkLog)
                    LogHelper.SendSavedLog();

                if (CommandSettings.MeasureElapsedTime)
                {
                    ConsoleWriter.WriteInfo("Total time: " + sw.Elapsed);
                    Log.LogDebug("Total time: " + sw.Elapsed);
                }

                return exitCode;
            }
            catch (GitLocalChangesException e)
            {
                Log?.LogWarning("Failed to " + GetType().Name.ToLower(), e);
                ConsoleWriter.WriteError(e.Message);
                return -1;
            }
            catch (CementException e)
            {
                Log?.LogError("Failed to " + GetType().Name.ToLower(), e);
                ConsoleWriter.WriteError(e.Message);
                return -1;
            }
            catch (Exception e)
            {
                Log?.LogError("Failed to " + GetType().Name.ToLower(), e);
                ConsoleWriter.WriteError(e.Message);
                ConsoleWriter.WriteError(e.StackTrace);
                return -1;
            }
        }

        private void CheckRequireYaml()
        {
            if (CommandSettings.Location == CommandLocation.RootModuleDirectory &&
                CommandSettings.RequireModuleYaml &&
                !File.Exists(Helper.YamlSpecFile))
                throw new CementException("This command require module.yaml file.\nUse convert-spec for convert old spec to module.yaml.");
        }

        private void SetWorkspace()
        {
            var cwd = Directory.GetCurrentDirectory();
            if (CommandSettings.Location == CommandLocation.WorkspaceDirectory)
            {
                if (!Helper.IsCementTrackedDirectory(cwd))
                    throw new CementTrackException(cwd + " is not cement workspace directory.");
                Helper.SetWorkspace(cwd);
            }

            if (CommandSettings.Location == CommandLocation.RootModuleDirectory)
            {
                if (!Helper.IsCurrentDirectoryModule(cwd))
                    throw new CementTrackException(cwd + " is not cement module directory.");
                Helper.SetWorkspace(Directory.GetParent(cwd).FullName);
            }

            if (CommandSettings.Location == CommandLocation.InsideModuleDirectory)
            {
                var currentModuleDirectory = Helper.GetModuleDirectory(Directory.GetCurrentDirectory());
                if (currentModuleDirectory == null)
                    throw new CementTrackException("Can't locate module directory");
                Helper.SetWorkspace(Directory.GetParent(currentModuleDirectory).FullName);
            }
        }

        private void InitLogging()
        {
            if (CommandSettings.LogFileName != null)
                LogHelper.InitializeFileAndElkLogging(CommandSettings.LogFileName, GetType().ToString());
            else if (!CommandSettings.NoElkLog)
                LogHelper.InitializeGlobalFileAndElkLogging(GetType().ToString());

            Log = LogManager.GetLogger(GetType());

            try
            {
                Log.LogInformation("Cement version: " + Helper.GetAssemblyTitle());
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void LogAndParseArgs(string[] args)
        {
            Log.LogDebug($"Parsing args: [{string.Join(" ", args)}] in {Directory.GetCurrentDirectory()}");
            ParseArgs(args);
            Log.LogDebug("OK parsing args");
        }

        protected abstract int Execute();
        protected abstract void ParseArgs(string[] args);
        public abstract string HelpMessage { get; }

        public bool IsHiddenCommand => CommandSettings.IsHiddenCommand;
    }
}