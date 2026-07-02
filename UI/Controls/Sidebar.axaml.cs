using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;

namespace UI.Controls;

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

        entry_All.Init(() => UpdateSelection(0));
        entry_Favs.Init(() => UpdateSelection(1));
        entry_Recent.Init(() => UpdateSelection(2));
        await entry_Cols.Init((cId) => UpdateSelection(3, cId), DependencyManager.GetService<ITaggingLogic>()!.GetCollections);
        await entry_Tags.Init((cId) => UpdateSelection(4, cId), DependencyManager.GetService<ITaggingLogic>()!.GetTags);
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
        {
            if (i == id)
            {
                cont_Entries.Children[i].Classes.RemoveRange(0, cont_Entries.Children[i].Classes.Count);
            }
            else
            {
                cont_Entries.Children[i].Classes.Add("Selected");
            }
        }

        await homepage!.SearchCards(search);
    }
}