using Models.Data;

namespace Models.Interfaces;

public interface IEditorLogic
{
    public Task<string[]> GetInstalledEditorVersions();
    public Task<EditorInstallInfo[]> GetInstalledEditorVersionsMoreInfo(CancellationToken token);

    public Task<bool> IsVersionInstalled(string? version);
    public Task<string?> GetEditorInstall(string? version);
    public Task<string[]> GetEditorLocations(bool recache);

    public Task LaunchProject(int id);
    public Task LaunchProject(ProjectInfo info);

    public Task DeriveProjectInfo(ProjectInfo info);

    public Task CreateProject(string name, string path, string version);
}
