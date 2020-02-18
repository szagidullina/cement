using System.IO;
using Common.YamlParsers;

namespace Common
{
    public static class ConfigurationParser
    {
        public static IConfigurationParser Create(FileInfo modulePath)
        {
            if (File.Exists(Path.Combine(modulePath.FullName, Helper.YamlSpecFile)))
            {
                return new ConfigurationYamlParser(modulePath);
            }

            if (File.Exists(Path.Combine(modulePath.FullName, ".cm", "spec.xml")))
            {
                return new ConfigurationXmlParser(modulePath);
            }

            return new NullConfigurationParser();
        }
    }
}