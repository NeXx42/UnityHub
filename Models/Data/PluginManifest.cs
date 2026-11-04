namespace Models.Data;

public struct PluginManifest
{
    public string Name { get; set; }
    public string Owner { get; set; }
    public string Entrypoint { get; set; }
    public string Version { get; set; }
}