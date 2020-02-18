namespace Common
{
    public class Package
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public PackageType Type { get; set; }

        public Package(string name, string url, PackageType type = PackageType.Git)
        {
            Name = name;
            Url = url;
            Type = type;
        }
    }

    public enum PackageType : byte
    {
        File = 0,
        Git = 1,
    }
}