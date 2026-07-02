using Models.Data;
using Models.Interfaces;

namespace Data.DataRepos;

public class MockDataRepo : IDataRepository
{
    public Dictionary<int, (ProjectCard, ProjectInfo)>? lookup;

    public Task Setup()
    {
        lookup = new Dictionary<int, (ProjectCard, ProjectInfo)>()
        {
            { 0, CreateCard(0, "test1", "/home/matth/Documents/Unity/SimpleMRPG", "/home/matth/Downloads/003db2ba-1df6-4b75-9271-0e6491b89551.png") },
            { 1, CreateCard(1, "test1", "/home/matth/Documents/Unity/SimpleMRPG", "/home/matth/Downloads/b33ee549-66fd-4fc8-ae0e-17ef59f3fd09.png") },
            { 2, CreateCard(2, "test1", "/home/matth/Documents/Unity/SimpleMRPG", "") },
            { 3, CreateCard(3, "test1", "/home/matth/Documents/Unity/SimpleMRPG", "") },
            { 4, CreateCard(4, "test1", "/home/matth/Documents/Unity/SimpleMRPG", "") },
        };


        return Task.CompletedTask;

        (ProjectCard, ProjectInfo) CreateCard(int id, string name, string dir, string icon)
        {
            return (
                new ProjectCard()
                {
                    id = id,
                    name = name,
                    directory = dir,
                    iconUrl = icon,
                },
                new ProjectInfo()
                {
                    id = id,
                    name = name,
                    directory = dir,
                    iconUrl = icon,
                }
            );
        }
    }

    public async Task<ProjectCard[]> GetProjectCards()
    {
        await Task.Delay(1000);
        return lookup!.Values.Select(v => v.Item1).ToArray();
    }

    public async Task<ProjectInfo> GetProjectInfo(int id)
    {
        await Task.Delay(1000);
        return lookup![id].Item2;
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

    public Task<ProjectCard[]> GetCardInfo(IEnumerable<int> ids)
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
}
