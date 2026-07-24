using Models.Data;
using Models.Enums;

namespace Models.Interfaces;

public interface IEditorLogic
{
    public Task<string[]> GetInstalledEditorVersions();

    public Task InstallEditor(EditorInfo version, HashSet<string> desiredModules, string? path);

    public Task<EditorInfo?> GetEditorMetadata(string versionName);
    public Task<(EditorInfo[], int)> SearchEditorDownloads(EditorFilterType filterType, string? filter, int page, int pageSize);
    public Task<EditorInstallInfo[]> GetEditorMetadataForDownloadedVersions(CancellationToken token);

    public void RegisterGlobalInstallProgressUpdate(Action<float?> callback);
    public Dictionary<EditorInfo, DownloadStatus> GetActiveInstalls();
    public void StopActiveInstall(string version);

    public Task<bool> IsVersionInstalled(string? version);
    public Task<string?> GetEditorInstall(string? version);
    public Task<string[]> GetEditorLocations(bool recache);

    public bool IsProjectRunning(int id);
    public Task LaunchProject(int id);
    public Task LaunchProject(ProjectInfo info);

    public Task<bool> CreateProject(ProjectCreationInfo info);
    public Task Delete(string versionName);

    public void BrowseToEditor(EditorInfo? info);
}
