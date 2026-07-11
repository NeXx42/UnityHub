using Models.Data;

namespace Models.Interfaces;

public interface IEditorLogic
{
    public Task<string[]> GetInstalledEditorVersions();

    public Task InstallEditor(EditorInfo version, int downloadId, string path);
    public Task<EditorInfo[]> GetEditorDownloads();
    public Task<EditorInstallInfo[]> GetInstalledEditorVersionsMoreInfo(CancellationToken token);

    public Task<bool> IsVersionInstalled(string? version);
    public Task<string?> GetEditorInstall(string? version);
    public Task<string[]> GetEditorLocations(bool recache);

    public bool IsProjectRunning(int id);
    public Task LaunchProject(int id);
    public Task LaunchProject(ProjectInfo info);

    public Task DeriveProjectInfo(ProjectInfo info);

    public Task CreateProject(ProjectCreationInfo info);
}
