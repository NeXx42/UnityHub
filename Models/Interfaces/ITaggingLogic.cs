using Models.Data;

namespace Models.Interfaces;

public interface ITaggingLogic
{
    public Task<CollectionData[]> GetTags();
    public Task<CollectionData[]> GetCollections();
}
