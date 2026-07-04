using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;
using UI.Popups;

namespace UI.Pages.HomePage;

public partial class MoreInfo : UserControl
{
    public ProjectInfo? info { get; private set; }

    private ReusableList<CollectionItem> tags;
    private ReusableList<CollectionItem> collections;

    public MoreInfo()
    {
        InitializeComponent();

        tags = new ReusableList<CollectionItem>(cont_Tags);
        collections = new ReusableList<CollectionItem>(cont_Collections);

        btn_OpenProject.RegisterClick(() => DependencyManager.GetService<IEditorLogic>()!.LaunchProject(info!));
        btn_OpenExplorer.RegisterClick(() => DependencyManager.GetService<IProjectLogic>()!.BrowseTo(info!));
    }

    public async Task Show(int id)
    {
        info = await DependencyManager.GetService<IProjectLogic>()!.GetProjectInfo(id);
        DataContext = info;

        if (info == null)
            return;

        ITaggingLogic logic = DependencyManager.GetService<ITaggingLogic>()!;

        Task.WaitAll([
            tags.DrawAsync(() => logic.MapTags(info.tags), (ui, _, dat) => ui.Init(dat)),
            collections.DrawAsync(() => logic.MapCollections(info.collections), (ui, _, dat) => ui.Init(dat)),
        ]);

        img.Source = await IconFetcher.GetImage(info.iconUrl);

        btn_AddTag.RegisterPopup(await new Popup_Collection().Init(
                logic.GetTags,
                AddTag,
                logic.CreateTag,
                () => btn_AddTag.IsOpen = false)
        );
        btn_AddCollection.RegisterPopup(await new Popup_Collection().Init(
            logic.GetCollections,
            AddCollection,
            logic.CreateCollection,
            () => btn_AddCollection.IsOpen = false)
        );
    }

    private async Task AddTag(CollectionData data)
    {
        if (info == null || info.tags.Contains(data.collectionId))
            return;

        info.tags.Add(data.collectionId);

        ITaggingLogic logic = DependencyManager.GetService<ITaggingLogic>()!;
        await logic.UpdateTag(info.id, data.collectionId, true);
        await tags.DrawAsync(() => logic.MapTags(info.tags), (ui, _, dat) => ui.Init(dat));
    }

    private async Task AddCollection(CollectionData data)
    {
        if (info == null || info.collections.Contains(data.collectionId))
            return;

        info.collections.Add(data.collectionId);

        ITaggingLogic logic = DependencyManager.GetService<ITaggingLogic>()!;
        await logic.UpdateTag(info.id, data.collectionId, true);
        await collections.DrawAsync(() => logic.MapCollections(info.collections), (ui, _, dat) => ui.Init(dat));
    }
}