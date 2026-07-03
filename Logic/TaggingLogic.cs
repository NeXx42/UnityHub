using Models.Data;
using Models.Interfaces;

namespace Logic;

public class TaggingLogic : ITaggingLogic
{
    private IDataRepository data => DependencyManager.GetService<IDataRepository>()!;

    private Dictionary<int, CollectionData>? cachedTags;
    private Dictionary<int, CollectionData>? cachedCollections;

    public async Task<CollectionData[]> GetCollections()
    {
        if (cachedCollections == null)
        {
            CollectionData[] cols = await data.GetCollections();
            cachedCollections = new Dictionary<int, CollectionData>(cols.Length);

            foreach (CollectionData col in cols)
                cachedCollections[col.collectionId] = col;
        }

        return cachedCollections!.Values.ToArray();
    }

    public async Task<CollectionData[]> GetTags()
    {
        if (cachedTags == null)
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
}
