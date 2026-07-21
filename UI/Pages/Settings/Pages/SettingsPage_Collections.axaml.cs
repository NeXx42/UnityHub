using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Helpers;
using UI.Modals;

namespace UI.Pages.Settings.Pages;

public partial class SettingsPage_Collections : UserControl, ISettingsPage
{
    private ReusableList<SettingsPage_Collections_Container> collections;
    private ReusableList<SettingsPage_Collections_Container> tags;

    public SettingsPage_Collections()
    {
        InitializeComponent();

        collections = new ReusableList<SettingsPage_Collections_Container>(cont_Collections);
        tags = new ReusableList<SettingsPage_Collections_Container>(cont_Tags);

        btn_AddCollection.RegisterClick(TryToCreateCollection);
        btn_AddTag.RegisterClick(TryToCreateTag);

        DependencyManager.GetService<ITaggingLogic>()!.RegisterCallback(OnChange);
    }

    public UserControl getControl => this;

    public async Task OnOpen()
    {
        await Task.WhenAll([
            RedrawCollections(),
            RedrawTags()
        ]);
    }

    private async Task RedrawCollections() => await collections.DrawAsync(DependencyManager.GetService<ITaggingLogic>()!.GetCollections, (ui, i, dat) => ui.Draw(dat, i));
    private async Task RedrawTags() => await tags.DrawAsync(DependencyManager.GetService<ITaggingLogic>()!.GetTags, (ui, i, dat) => ui.Draw(dat, i));

    private async Task TryToCreateTag()
    {
        TagData? newTag = await MainWindow.ShowModalAndWait<CreateCollectionModal, TagData>(async m =>
        {
            return await m.Init<TagData>(null);
        });

        if (newTag == null)
            return;

        await DependencyManager.GetService<ITaggingLogic>()!.CreateTag(newTag);
        await RedrawTags();
    }

    private async Task TryToCreateCollection()
    {
        CollectionData? newCol = (CollectionData?)await MainWindow.ShowModalAndWait<CreateCollectionModal, TagData>(async m =>
        {
            return await m.Init<CollectionData>(null);
        });

        if (newCol == null)
            return;

        await DependencyManager.GetService<ITaggingLogic>()!.CreateCollection(newCol);
        await RedrawCollections();
    }

    private void OnChange(int? _, string msg)
    {
        switch (msg)
        {
            case nameof(ITaggingLogic.DeleteCollection):
                RedrawCollections().Wrap();
                break;

            case nameof(ITaggingLogic.DeleteTag):
                RedrawTags().Wrap();
                break;
        }
    }
}