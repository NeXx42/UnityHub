using Models.Data;

namespace Models.Interfaces;

public interface IProjectLogic
{
    public void RegisterCallback(Action<string> callback);

    public Task<(ProjectInfo[], int total)> Search(ProjectSearch search);
    public Task<ProjectInfo?> GetProjectInfo(int? id);

    public Task<string[]> GetProjectVersions();

    public void OpenIDE(ProjectInfo info);
    public void BrowseTo(ProjectInfo info);
    public void MoveProject(ProjectInfo info);
    public void BrowseTerminal(ProjectInfo info);
    public void DuplicateProject(ProjectInfo info);

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

    public Task DeleteCard(ProjectInfo info);
    public Task<bool> TrySwitchVersion(ProjectInfo info, string to);
}
