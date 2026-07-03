using Models.Data;

namespace Models.Interfaces;

public interface ITaggingLogic
{
    public Task<CollectionData[]> GetTags();
    public Task<CollectionData[]> GetCollections();

    public Task<CollectionData[]> MapTags(IEnumerable<int> from);
    public Task<CollectionData[]> MapCollections(IEnumerable<int> from);
}
