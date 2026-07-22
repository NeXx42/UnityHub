using Models.Data;
using Models.Enums;

namespace Models.Interfaces;

public interface IEditorLogic
{
    public Task<string[]> GetInstalledEditorVersions();

    public Task InstallEditor(EditorInfo version, string path);
    public Task<EditorInstallInfo[]> GetInstalledEditorVersionsMoreInfo(CancellationToken token);
    public Task<(EditorInfo[], int)> GetEditorDownloads(EditorFilterType filterType, string? filter, int page, int pageSize);

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
}
