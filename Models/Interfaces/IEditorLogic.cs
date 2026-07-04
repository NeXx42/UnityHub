using Models.Data;

namespace Models.Interfaces;

public interface IEditorLogic
{
    public bool IsVersionInstalled(string? version);
    public string? GetEditorInstall(string? version);
    public string[] GetInstalledEditorVersions();

    public Task LaunchProject(int id);
    public Task LaunchProject(ProjectInfo info);

    public Task DeriveProjectInfo(ProjectInfo info);
}
