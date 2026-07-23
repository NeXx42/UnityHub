namespace Models.Data;

public class EditorInstallInfo : EditorInfo
{
    public required string installLocation;

    public BuiltInPackage[] builtInPackages = [];
    public HashSet<string> installedPackages = new();


    public struct BuiltInPackage
    {
        public string name { get; set; }
        public string displayName { get; set; }

        public string version { get; set; }
        public string unity { get; set; }
        public string[] keywords { get; set; }
        public string category { get; set; }
        public string description { get; set; }
        public Dictionary<string, string>? dependencies { get; set; }
    }
}
