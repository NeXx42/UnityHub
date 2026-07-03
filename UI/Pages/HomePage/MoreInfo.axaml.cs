using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;

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

        Task.WaitAll([
            tags.DrawAsync(() => DependencyManager.GetService<ITaggingLogic>()!.MapTags(info.tags), (ui, _, dat) => ui.Init(dat)),
            collections.DrawAsync(() => DependencyManager.GetService<ITaggingLogic>()!.MapCollections(info.collections), (ui, _, dat) => ui.Init(dat)),
        ]);

        img.Source = await IconFetcher.GetImage(info.iconUrl);

    }
}