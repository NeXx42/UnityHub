using Models.Data;
using Models.Interfaces;

namespace Logic;

public class TaggingLogic : ITaggingLogic
{
    private static string[] collectionColours = [
        "#f0546c",
        "#f0a84e",
        "#f0c94e",
        "#3ddc84",
        "#4ecfc0",
        "#4ea8f0",
        "#6c7bf0",
        "#b47cf0",
        "#e07cf0",
    ];

    private IDataRepository data => DependencyManager.GetService<IDataRepository>()!;

    private Dictionary<int, CollectionData>? cachedTags;
    private Dictionary<int, CollectionData>? cachedCollections;

    private Action<int?, string>? callbacks;

    public void RegisterCallback(Action<int?, string> callback)
    {
        callbacks += callback;
    }

    public async Task<CollectionData[]> GetCollections() => await GetCollections(false);
    public async Task<CollectionData[]> GetCollections(bool forceRecache)
    {
        if (cachedCollections == null || forceRecache)
        {
            CollectionData[] cols = await data.GetCollections();
            cachedCollections = new Dictionary<int, CollectionData>(cols.Length);

            foreach (CollectionData col in cols)
                cachedCollections[col.collectionId] = col;
        }

        return cachedCollections!.Values.ToArray();
    }

    public async Task<CollectionData[]> GetTags() => await GetTags(false);
    public async Task<CollectionData[]> GetTags(bool forceRecache)
    {
        if (cachedTags == null || forceRecache)
        {
            CollectionData[] tags = await data.GetTags();
            cachedTags = new Dictionary<int, CollectionData>(tags.Length);

            foreach (CollectionData col in tags)
                cachedTags[col.collectionId] = col;
        }

        return cachedTags!.Values.ToArray();
    }

    public async Task<CollectionData[]> MapTags(IEnumerable<int> from)
    {
        if (cachedTags == null)
            await GetTags();

        if (cachedTags == null || from.Count() == 0) // failed recache?
            return [];

        List<CollectionData> res = new List<CollectionData>();

        foreach (int id in from)
            if (cachedTags.TryGetValue(id, out CollectionData? dat) && dat != null)
                res.Add(dat);

        return res.ToArray();
    }

    public async Task<CollectionData[]> MapCollections(IEnumerable<int> from)
    {
        if (cachedCollections == null)
            await GetCollections();

        if (cachedCollections == null || from.Count() == 0) // failed recache?
            return [];

        List<CollectionData> res = new List<CollectionData>();

        foreach (int id in from)
            if (cachedCollections.TryGetValue(id, out CollectionData? dat) && dat != null)
                res.Add(dat);

        return res.ToArray();
    }

    public async Task UpdateTag(int projId, int tagId, bool to)
    {
        await data.ToggleTag(projId, tagId, to);
        callbacks?.Invoke(projId, nameof(UpdateTag));
    }
    public async Task UpdateCollection(int projId, int colId, bool to)
    {
        await data.ToggleCollection(projId, colId, to);
        callbacks?.Invoke(projId, nameof(UpdateCollection));
    }

    public async Task CreateTag(CollectionData src)
    {
        int colourId = (cachedTags?.Count ?? 0) % collectionColours.Length;
        await data.CreateTag(new CollectionData
        {
            collectionId = 1,
            collectionName = src.collectionName,
            colour = collectionColours[colourId],
            type = "tag",
        });

        _ = await GetTags(true);
        callbacks?.Invoke(null, nameof(CreateTag));
    }

    public async Task CreateCollection(CollectionData src)
    {
        int colourId = (cachedCollections?.Count ?? 0) % collectionColours.Length;
        await data.CreateCollection(new CollectionData
        {
            collectionId = 1,
            collectionName = src.collectionName,
            colour = collectionColours[colourId],
            type = "collection"
        });

        _ = await GetCollections(true);
        callbacks?.Invoke(null, nameof(CreateCollection));
    }
}
