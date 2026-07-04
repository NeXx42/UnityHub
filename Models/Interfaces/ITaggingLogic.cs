using Models.Data;

namespace Models.Interfaces;

public interface ITaggingLogic
{
    /// <summary>
    /// project id, message
    /// </summary>
    /// <param name="callback"></param>
    public void RegisterCallback(Action<int?, string> callback);

    public Task<CollectionData[]> GetTags();
    public Task<CollectionData[]> GetTags(bool forceRecache);

    public Task<CollectionData[]> GetCollections();
    public Task<CollectionData[]> GetCollections(bool forceRecache);

    public Task<CollectionData[]> MapTags(IEnumerable<int> from);
    public Task<CollectionData[]> MapCollections(IEnumerable<int> from);

    public Task UpdateTag(int projId, int tagId, bool to);
    public Task UpdateCollection(int projId, int colId, bool to);

    public Task CreateTag(CollectionData data);
    public Task CreateCollection(CollectionData data);
}
