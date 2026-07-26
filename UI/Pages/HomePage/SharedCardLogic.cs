using System.Threading.Tasks;
using Logic;
using Models.Data;
using Models.Enums;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;

namespace UI.Pages.HomePage;

public static class SharedCardLogic
{
    public static async Task DrawTagList(ProjectInfo activeCard, ReusableList<CollectionItem> tags)
    {
        TagData[] data;
        ITaggingLogic logic = DependencyManager.GetService<ITaggingLogic>()!;

        if ((await DependencyManager.GetService<IConfigLogic>()!.Get(ConfigEntry.IncludeCollectionInTagList, Config_EnabledStatus.Enabled)) == Config_EnabledStatus.Enabled)
            data = [.. await logic.MapCollections([activeCard!.collectionId]), .. await logic.MapTags(activeCard!.tags)];
        else
            data = await logic.MapTags(activeCard!.tags);

        tags.Draw(data, (ui, _, dat) =>
        {
            ui.Init(dat);
        }, 3);
    }
}
