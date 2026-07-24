using Models.Data;
using Models.Enums;
using Models.Interfaces;

namespace Data.DataRepos;

public class MockDataRepo : IDataRepository
{
    public Dictionary<int, ProjectInfo>? lookup;

    private List<TagData> tags = new List<TagData>();
    private List<CollectionData> collections = new List<CollectionData>();

    public Task Setup()
    {
        lookup = new Dictionary<int, ProjectInfo>()
        {
            { 0, CreateCard(0, "test1", "/home/matth/Documents/Unity/SimpleMRPG", "/home/matth/Downloads/003db2ba-1df6-4b75-9271-0e6491b89551.png") },
            { 1, CreateCard(1, "test1", "/home/matth/Documents/Unity/SimpleMRPG", "/home/matth/Downloads/b33ee549-66fd-4fc8-ae0e-17ef59f3fd09.png") },
            { 2, CreateCard(2, "test1", "/home/matth/Documents/Unity/SimpleMRPG", "") },
            { 3, CreateCard(3, "test1", "/home/matth/Documents/Unity/SimpleMRPG", "") },
            { 4, CreateCard(4, "test1", "/home/matth/Documents/Unity/SimpleMRPG", "") },
        };

        return Task.CompletedTask;

        ProjectInfo CreateCard(int id, string name, string dir, string icon)
        {
            return new ProjectInfo()
            {
                id = id,
                name = name,
                directory = dir,
                iconUrl = icon,
                collectionId = (int)DefaultCollectionIds.InDevelopment
            };
        }
    }

    public async Task<ProjectInfo[]> GetProjectInfo(IEnumerable<int> cards)
    {
        await Task.Delay(1000);
        return lookup!.Values.Select(v => v).ToArray();
    }

    public async Task<ProjectInfo?> GetProjectInfo(int id)
    {
        await Task.Delay(1000);
        return lookup![id];
    }

    public Task CreateCard(ProjectInfo info)
    {
        throw new NotImplementedException();
    }

    public Task CreateCards(IEnumerable<ProjectInfo> cards)
    {
        throw new NotImplementedException();
    }

    public Task<(int[], int)> Search(ProjectSearch search)
    {
        throw new NotImplementedException();
    }

    public Task<TagData[]> GetTags() => Task.FromResult(tags.ToArray());
    public Task<CollectionData[]> GetCollections() => Task.FromResult(collections.ToArray());

    public Task Migrate(IEnumerable<int> ids)
    {
        throw new NotImplementedException();
    }

    public Task ToggleTag(int projId, int tagId, bool to)
    {
        throw new NotImplementedException();
    }

    public Task ToggleCollection(int projId, int colId, bool to)
    {
        throw new NotImplementedException();
    }

    public Task CreateTag(TagData src)
    {
        tags.Add(src);
        return Task.CompletedTask;
    }

    public Task CreateCollection(CollectionData src)
    {
        collections.Add(src);
        return Task.CompletedTask;
    }

    public Task<string[]> GetProjectVersions()
    {
        throw new NotImplementedException();
    }

    public Task<string?[]> GetConfigValue(string key)
    {
        throw new NotImplementedException();
    }

    public Task SetConfigValue(string key, string? value)
    {
        throw new NotImplementedException();
    }

    public Task DeleteConfigValue(string key)
    {
        throw new NotImplementedException();
    }

    public Task SetEditorInfo(Dictionary<string, string> versionJson)
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<string, string>> GetEditorInfo(IEnumerable<string> versions)
    {
        throw new NotImplementedException();
    }

    public Task Migrate(IEnumerable<ProjectInfo> ids)
    {
        throw new NotImplementedException();
    }

    public Task DeleteCard(IEnumerable<int> ids)
    {
        throw new NotImplementedException();
    }

    public Task UpdateLastOpened(ProjectInfo info)
    {
        throw new NotImplementedException();
    }

    public Task UpdateProjectProperties(ProjectInfo info, IEnumerable<string> properties)
    {
        throw new NotImplementedException();
    }

    public Task UpdateProjectProperties(IEnumerable<ProjectInfo> updates, IEnumerable<string> properties)
    {
        throw new NotImplementedException();
    }

    public Task SetCollection(int projId, int colId)
    {
        throw new NotImplementedException();
    }

    Task<int> IDataRepository.CreateCard(ProjectInfo info)
    {
        throw new NotImplementedException();
    }

    Task<Dictionary<string, int>> IDataRepository.CreateCards(IEnumerable<ProjectInfo> cards)
    {
        throw new NotImplementedException();
    }

    public Task DeleteTag(int id)
    {
        throw new NotImplementedException();
    }

    public Task DeleteCollection(int id)
    {
        throw new NotImplementedException();
    }
}
