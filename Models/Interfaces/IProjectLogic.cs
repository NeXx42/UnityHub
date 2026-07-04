using Models.Data;

namespace Models.Interfaces;

public interface IProjectLogic
{
    public Task<(ProjectInfo[], int total)> Search(ProjectSearch search);
    public Task<ProjectInfo?> GetProjectInfo(int id);

    public Task BrowseTo(int id);
    public Task BrowseTo(ProjectInfo info);

    public Task<ProjectInfo[]> TryToUpload(string[] folders);

    public Task UploadCardsPrimitive(ProjectInfo[] cards);
}
