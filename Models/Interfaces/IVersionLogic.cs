namespace Models.Interfaces;

public interface IVersionLogic
{
    public string? getCurrentVersion { get; }
    public bool hasUpdateAvailable { get; }
}
