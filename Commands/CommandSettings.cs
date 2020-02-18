namespace Commands
{
    public class CommandSettings
    {
        public string LogPerfix;
        public string LogFileName;
        public bool MeasureElapsedTime;
        public bool RequireModuleYaml;
        public bool IsHiddenCommand = false;
        public bool NoElkLog = false;
        public CommandLocation Location;
    }
}