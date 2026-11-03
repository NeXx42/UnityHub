namespace Models.Interfaces;

public interface IVersionLogic
{
    public string? getCurrentVersion { get; }
    public string? getLatestVersion { get; }
    public bool hasUpdateAvailable { get; }

    public bool GetUpdateInfo(out string latestVersion, out string updateMessage);
}
