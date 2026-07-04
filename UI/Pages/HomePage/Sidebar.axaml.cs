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

public partial class Sidebar : UserControl
{
    private Pages.HomePage.Page? homepage;

    public Sidebar()
    {
        InitializeComponent();
    }

    public async Task Init(Pages.HomePage.Page homepage)
    {
        this.homepage = homepage;

        DependencyManager.GetService<ITaggingLogic>()!.RegisterCallback(TaggingChange);

        entry_All.Init(() => UpdateSelection(0));
        entry_Favs.Init(() => UpdateSelection(1));
        entry_Recent.Init(() => UpdateSelection(2));

        await Task.WhenAll([
            DrawTags(),
            DrawCollections(),
            UpdateSelection(0)
        ]);
    }


    private async Task UpdateSelection(int id, int? subArg = null)
    {
        ProjectSearch search = new ProjectSearch();

        switch (id)
        {
            case 1: // Favs
                break;

            case 2: // Recent
                break;

            case 3: // Collections
                search.collections = [subArg!.Value];
                break;

            case 4: // Tags
                search.tags = [subArg!.Value];
                break;
        }

        for (int i = 0; i < cont_Entries.Children.Count; i++)
            (cont_Entries.Children[i] as ISidebarControl)?.setSelected = i == id;

        await homepage!.SearchCards(search);
    }

    private void TaggingChange(int? projId, string msg)
    {
        if (projId.HasValue)
            return;

        switch (msg)
        {
            case nameof(ITaggingLogic.CreateCollection):
                DrawCollections().Wrap();
                break;

            case nameof(ITaggingLogic.CreateTag):
                DrawTags().Wrap();
                break;
        }
    }

    private async Task DrawTags() => await entry_Tags.Init((cId) => UpdateSelection(4, cId), DependencyManager.GetService<ITaggingLogic>()!.GetTags);
    private async Task DrawCollections() => await entry_Cols.Init((cId) => UpdateSelection(3, cId), DependencyManager.GetService<ITaggingLogic>()!.GetCollections);
}