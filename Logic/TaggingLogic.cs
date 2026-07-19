using Logic.Tagging;
using Microsoft.VisualBasic;
using Models.Data;
using Models.Enums;
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

    private static CollectionData[] defaultCollections = [
        new CollectionData(){
            collectionId = (int)DefaultCollectionIds.InDevelopment,
            collectionName = "In Development",
            colour = "#3ddc84",
            handlingType = CollectionHandlingTypes.None,
        },
        new CollectionData(){
            collectionId = (int)DefaultCollectionIds.Archive,
            collectionName = "Archived",
            colour = "#f0c94e",
            handlingType = CollectionHandlingTypes.Archive,
        },
        new CollectionData(){
            collectionId = (int)DefaultCollectionIds.Released,
            collectionName = "Released",
            colour = "#4ea8f0",
            handlingType = CollectionHandlingTypes.Release,
        },
    ];

    private IDataRepository data => DependencyManager.GetService<IDataRepository>()!;

    private Dictionary<int, TagData>? cachedTags;
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
            CollectionData[] cols = [.. defaultCollections, .. await data.GetCollections()];
            cachedCollections = new Dictionary<int, CollectionData>(cols.Length);

            foreach (CollectionData col in cols)
                cachedCollections[col.collectionId] = col;
        }

        return cachedCollections!.Values.ToArray();
    }

    public async Task<TagData[]> GetTags() => await GetTags(false);
    public async Task<TagData[]> GetTags(bool forceRecache)
    {
        if (cachedTags == null || forceRecache)
        {
            TagData[] tags = await data.GetTags();
            cachedTags = new Dictionary<int, TagData>(tags.Length);

            foreach (TagData col in tags)
                cachedTags[col.collectionId] = col;
        }

        return cachedTags!.Values.ToArray();
    }

    public async Task<TagData[]> MapTags(IEnumerable<int> from)
    {
        if (cachedTags == null)
            await GetTags();

        if (cachedTags == null || from.Count() == 0) // failed recache?
            return [];

        List<TagData> res = new List<TagData>();

        foreach (int id in from)
            if (cachedTags.TryGetValue(id, out TagData? dat) && dat != null)
                res.Add(dat);

        return res.ToArray();
    }

    public async Task<CollectionData[]> MapCollections(IEnumerable<int> from)
    {
        if (cachedCollections == null)
            await GetCollections();

        if (cachedCollections == null || from.Count() == 0) // failed recache?
            return [];

        List<CollectionData> res = new(from.Count());

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
    public async Task<bool> TryToChangeCollection(ProjectInfo project, int colId)
    {
        if (project.collectionId == colId)
            return false;

        CollectionData[] colletionData = await MapCollections([project.collectionId, colId]);
        CollectionData? oldCollection;
        CollectionData newCollection;

        if (colletionData.Length != 2)
        {
            if (colletionData.FirstOrDefault()?.collectionId == colId) // couldnt resolve previous collection, but allow to continue
            {
                if (await DependencyManager.ui!.ShowConfirmationBox(
                    "Couldn't resolve previous collection",
                    "The previous collection couldn't be resolved, the collection handling could result in invalid logic when translating between collections.\nDo you want to continue?",
                    new ConfirmationButton()
                    {
                        label = "Cancel",
                    },
                    new ConfirmationButton()
                    {
                        label = "Contine",
                        className = "Primary"
                    }
                ) != 1)
                    return false;

                newCollection = colletionData.Single();
            }
            else // either none, or the desired wasnt the only thing returned?, could be resolved. cannot continue
            {
                await DependencyManager.ui!.ShowMessageBox("Failed to update collection", "The desired collection could not be resolved");
                return false;
            }
        }
        else
        {
            oldCollection = colletionData[0];
            newCollection = colletionData[1];
        }

        CollectionHandler_Base convertor;

        switch (newCollection.handlingType)
        {
            default: convertor = new CollectionHandler_None(); break;
            case Models.Enums.CollectionHandlingTypes.Release: convertor = new CollectionHandler_Released(); break;
            case Models.Enums.CollectionHandlingTypes.Archive: convertor = new CollectionHandler_Archive(); break;
        }

        string? confirmation = convertor.getConfirmationMessage;

        if (!string.IsNullOrEmpty(confirmation))
        {
            if (await DependencyManager.ui!.ShowConfirmationBox(
                "Convert collection",
                confirmation,
                new ConfirmationButton()
                {
                    label = "Cancel",
                },
                new ConfirmationButton()
                {
                    label = "Contine",
                    className = "Primary"
                }
            ) != 1)
                return false;
        }

        Exception? e = await DependencyManager.ui!.LoadProgressive("Converting", convertor.GetTransformations(project));

        if (e != null)
        {
            await DependencyManager.ui.ShowMessageBox("Failed to convert project", $"An error occured while converting the collection. \n{e.Message}");
            return false;
        }

        project.collectionId = colId;
        await UpdateCollection(project.id, colId);

        return true;
    }

    private async Task UpdateCollection(int projId, int colId)
    {
        await data.SetCollection(projId, colId);
        callbacks?.Invoke(projId, nameof(UpdateCollection));
    }

    public async Task CreateTag(TagData src)
    {
        int colourId = (cachedTags?.Count ?? 0) % collectionColours.Length;
        await data.CreateTag(new TagData
        {
            collectionId = 1,
            collectionName = src.collectionName,
            colour = collectionColours[colourId],
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
        });

        _ = await GetCollections(true);
        callbacks?.Invoke(null, nameof(CreateCollection));
    }
}
