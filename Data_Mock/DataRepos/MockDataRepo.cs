using Models.Data;
using Models.Interfaces;

namespace Data.DataRepos;

public class MockDataRepo : IDataRepository
{
    public Dictionary<int, ProjectInfo>? lookup;

    public Task Setup()
    {
        lookup = new Dictionary<int, ProjectInfo>()
        {
            { 0, CreateCard(0, "test1", "/home/matth/Documents/Unity/SimpleMRPG", "/home/matth/Downloads/003db2ba-1df6-4b75-9271-0e6491b89551.png") },
            { 1, CreateCard(1, "test1", "/home/matth/Documents/Unity/SimpleMRPG", "/home/matth/Downloads/b33ee549-66fd-4fc8-ae0e-17ef59f3fd09.png") },
            { 2, CreateCard(2, "test1", "/home/matth/Documents/Unity/SimpleMRPG", "") },
            { 3, CreateCard(3, "test1", "/home/matth/Documents/Unity/SimpleMRPG", "") },
            { 4, CreateCard(4, "test1", "/home/matth/Documents/Unity/SimpleMRPG", "") },
        }
        ;


        return Task.CompletedTask;

        ProjectInfo CreateCard(int id, string name, string dir, string icon)
        {
            return new ProjectInfo()
            {
                id = id,
                name = name,
                directory = dir,
                iconUrl = icon,
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

    public Task<CollectionData[]> GetTags()
    {
        throw new NotImplementedException();
    }

    public Task<CollectionData[]> GetCollections()
    {
        throw new NotImplementedException();
    }

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

    public Task CreateTag(CollectionData src)
    {
        throw new NotImplementedException();
    }

    public Task CreateCollection(CollectionData src)
    {
        throw new NotImplementedException();
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
}
