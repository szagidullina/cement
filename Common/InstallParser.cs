using System.IO;

namespace Common
{
    public static class InstallParser
    {
        public static InstallData Get(string module, string configuration)
        {
            var yamlSpecFile = Path.Combine(Helper.CurrentWorkspace, module, Helper.YamlSpecFile);

            if (File.Exists(yamlSpecFile))
                return new InstallCollector(Directory.GetParent(yamlSpecFile).FullName).Get(configuration);

            var xmlSpecFile = Path.Combine(Helper.CurrentWorkspace, module, ".cm", "spec.xml");
            if (File.Exists(xmlSpecFile))
                return new InstallXmlParser(File.ReadAllText(xmlSpecFile), module).Get(configuration);

            return new InstallData();
        }
    }
}