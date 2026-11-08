namespace Models.Data;

public struct PluginManifest
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }
    public required string Owner { get; set; }
    public required string Entrypoint { get; set; }
    public required string Version { get; set; }
}