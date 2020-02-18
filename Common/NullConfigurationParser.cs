using System.Collections.Generic;

namespace Common
{
    public class NullConfigurationParser : IConfigurationParser
    {
        public IList<string> GetConfigurations()
        {
            return new[] {"full-build"};
        }

        public bool ConfigurationExists(string configName)
        {
            return configName.Equals("full-build");
        }

        public string GetDefaultConfigurationName()
        {
            return "full-build";
        }

        public IList<string> GetParentConfigurations(string configName)
        {
            return new List<string>();
        }

        public Dictionary<string, IList<string>> GetConfigurationsHierarchy()
        {
            return new Dictionary<string, IList<string>> {{"full-build", new List<string>()}};
        }
    }
}