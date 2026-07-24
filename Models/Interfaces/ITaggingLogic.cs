using Models.Data;

namespace Models.Interfaces;

public interface ITaggingLogic
{
    /// <summary>
    /// project id, message
    /// </summary>
    /// <param name="callback"></param>
    public void RegisterCallback(Action<int?, string> callback);

    public Task<TagData[]> GetTags();
    public Task<TagData[]> GetTags(bool forceRecache);

    public Task<CollectionData[]> GetCollections();
    public Task<CollectionData[]> GetCollections(bool forceRecache);

    public Task<TagData[]> MapTags(IEnumerable<int> from);
    public Task<CollectionData[]> MapCollections(IEnumerable<int> from);

    public Task UpdateTag(int projId, int tagId, bool to);
    public Task<bool> TryToChangeCollection(ProjectInfo project, int colId);

    public Task CreateOrUpdateTag(TagData data);
    public Task CreateOrUpdateCollection(CollectionData data);

    public Task DeleteTag(int id);
    public Task DeleteCollection(int id);

    public int GetTagCount();
    public int GetCollectionCount();
}
