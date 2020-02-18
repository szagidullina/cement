using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common;

namespace Commands
{
    public class CheckPreCommitCommand : CommandBase
    {
        public CheckPreCommitCommand()
            : base(
                new CommandSettings
                {
                    LogPerfix = "CHECK-PRE-COMMIT",
                    LogFileName = null,
                    MeasureElapsedTime = false,
                    RequireModuleYaml = false,
                    Location = CommandLocation.RootModuleDirectory,
                    IsHiddenCommand = true
                })
        {
        }

        protected override int Execute()
        {
            var cwd = Directory.GetCurrentDirectory();
            var moduleName = Path.GetFileName(cwd);

            var exitCode = 0;
            foreach (var file in GetChangedFiles(moduleName))
            {
                if (IsValid(file))
                    continue;

                exitCode = -1;
                Console.WriteLine("Bad encoding in file: " + file);
            }

            return exitCode;
        }

        private IEnumerable<string> GetChangedFiles(string moduleName)
        {
            var repo = new GitRepository(moduleName, Helper.CurrentWorkspace, Log);

            return repo.GetFilesForCommit()
                .Where(file => file.EndsWith(".cs") && File.Exists(file))
                .Distinct();
        }

        private static bool IsValid(string file)
        {
            var bytes = File.ReadAllBytes(file);
            var hasBom = FileHasUtf8Bom(bytes);

            if (hasBom)
                return true;

            return !FileHasNonAsciiSymbols(bytes);
        }

        private static bool FileHasNonAsciiSymbols(IEnumerable<byte> fileBytes)
        {
            return fileBytes.Any(b => b > 127);
        }

        private static bool FileHasUtf8Bom(byte[] fileBytes)
        {
            var preamble = new UTF8Encoding(true).GetPreamble();

            if (fileBytes.Length < preamble.Length)
                return false;

            for (var i = 0; i < preamble.Length; i++)
            {
                if (fileBytes[i] != preamble[i])
                    return false;
            }

            return true;
        }

        protected override void ParseArgs(string[] args)
        {
        }

        public override string HelpMessage => @"
    Checks that commit is good

    Usage:
        cm check-pre-commit
";
    }
}