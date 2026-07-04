using Models.Data;

namespace Models.Interfaces;

public interface IDataRepository
{
    public Task Setup();
    public Task Migrate(IEnumerable<int> ids);

    public Task<(int[], int)> Search(ProjectSearch search);

    public Task<ProjectInfo?> GetProjectInfo(int id);
    public Task<ProjectInfo[]> GetProjectInfo(IEnumerable<int> ids);

    public Task CreateCard(ProjectInfo info);
    public Task CreateCards(IEnumerable<ProjectInfo> cards);

    public Task<CollectionData[]> GetTags();
    public Task<CollectionData[]> GetCollections();

    public Task ToggleTag(int projId, int tagId, bool to);
    public Task ToggleCollection(int projId, int colId, bool to);

    public Task CreateTag(CollectionData src);
    public Task CreateCollection(CollectionData src);
}
