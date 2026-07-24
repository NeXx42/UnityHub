using Models.Data;

namespace Models.Interfaces;

public interface IProjectLogic
{
    public void RegisterCallback(Action<string> callback);

    public Task<(ProjectInfo[], int total)> Search(ProjectSearch search);
    public Task<ProjectInfo?> GetProjectInfo(int? id);

    public Task<string[]> GetProjectVersions();

    public Task OpenIDE(ProjectInfo info);
    public void BrowseTo(ProjectInfo info);
    public LoadRequest[] MoveProject(ProjectInfo info, string to);
    public Task BrowseTerminal(ProjectInfo info);
    public LoadRequest[] DuplicateProject(ProjectInfo info, string newName, string newDirectory);

    public Task<ProjectInfo[]> VerifyProjectPrimative(IEnumerable<string> folders);
    public Task<ProjectInfo?> VerifyProjectPrimative(ProjectInfo info);

    public LoadRequest[] DeriveProjectInfo(ProjectInfo info, bool recache);

    /// <summary>
    /// expect data interface to update the id to reflect the database
    /// </summary>
    /// <param name="cards"></param>
    /// <returns></returns>
    public Task UploadCardsPrimitive(IEnumerable<ProjectInfo> cards);

    public Task UpdateProperties(ProjectInfo info, IEnumerable<string> props);
    public Task UpdateProperties(IEnumerable<ProjectInfo> elements, IEnumerable<string> props);

    public LoadRequest[] DeleteCard(ProjectInfo info);
    public Task<bool> TrySwitchVersion(ProjectInfo info, string to);
}
