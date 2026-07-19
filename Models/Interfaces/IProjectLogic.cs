using Models.Data;

namespace Models.Interfaces;

public interface IProjectLogic
{
    public void RegisterCallback(Action<string> callback);

    public Task<(ProjectInfo[], int total)> Search(ProjectSearch search);
    public Task<ProjectInfo?> GetProjectInfo(int id);

    public Task<string[]> GetProjectVersions();

    public Task BrowseTo(int id);
    public Task BrowseTo(ProjectInfo info);

    public Task<ProjectInfo[]> VerifyProjectPrimative(IEnumerable<string> folders);
    public Task<ProjectInfo?> VerifyProjectPrimative(ProjectInfo info);

    public LoadRequest[] DeriveProjectInfo(ProjectInfo info, bool recache);

    public Task UploadCardsPrimitive(IEnumerable<ProjectInfo> cards);

    public Task UpdateProperties(ProjectInfo info, IEnumerable<string> props);
    public Task UpdateProperties(IEnumerable<ProjectInfo> elements, IEnumerable<string> props);

    public Task DeleteCard(ProjectInfo info);
    public Task<bool> TrySwitchVersion(ProjectInfo info, string to);
}
