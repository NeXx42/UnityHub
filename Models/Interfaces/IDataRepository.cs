using Models.Data;
using Models.Enums;

namespace Models.Interfaces;

public interface IDataRepository
{
    public Task Setup();

    public Task<(int[], int)> Search(ProjectSearch search);

    public Task<ProjectInfo?> GetProjectInfo(int id);
    public Task<ProjectInfo[]> GetProjectInfo(IEnumerable<int> ids);

    public Task UpdateProjectProperties(ProjectInfo info, IEnumerable<string> properties);
    public Task UpdateProjectProperties(IEnumerable<ProjectInfo> updates, IEnumerable<string> properties);

    public Task<int> CreateCard(ProjectInfo info);
    public Task<Dictionary<string, int>> CreateCards(IEnumerable<ProjectInfo> cards);
    public Task DeleteCard(IEnumerable<int> ids);



    public Task<TagData[]> GetTags();
    public Task<CollectionData[]> GetCollections();

    public Task ToggleTag(int projId, int tagId, bool to);
    public Task SetCollection(int projId, int colId);

    public Task CreateOrUpdateTag(TagData src);
    public Task CreateOrUpdateCollection(CollectionData src);

    public Task DeleteTag(int id);
    public Task DeleteCollection(int id);



    public Task<string[]> GetProjectVersions();
    public Task SetEditorInfo(Dictionary<string, string> versionJson);
    public Task<Dictionary<string, string>> GetEditorInfo(IEnumerable<string> versions);

    public Task<string?[]> GetConfigValue(string key);
    public Task SetConfigValue(string key, string? value);
    public Task DeleteConfigValue(string key);
}
