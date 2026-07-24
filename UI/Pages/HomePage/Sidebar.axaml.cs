using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Enums;
using Models.Interfaces;
using Tmds.DBus.Protocol;
using UI.Controls;
using UI.Helpers;

namespace UI.Pages.HomePage;

public partial class Sidebar : UserControl
{
    private Pages.HomePage.Page? homepage;
    private ReusableList<Sidebar_DriveUsage> storageUsage;

    public Sidebar()
    {
        InitializeComponent();

        storageUsage = new ReusableList<Sidebar_DriveUsage>(cont_Storage);
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
            UpdateSelection(0),
        ]);

        DrawStorageUsage().Wrap();
    }


    private async Task UpdateSelection(int id, int? subArg = null)
    {
        homepage!.activeSearch.Reset();

        switch (id)
        {
            case 1: // Favs
                homepage!.activeSearch.requiredFavs = true;
                break;

            case 2: // Recent
                homepage!.activeSearch.requiredOpened = true;
                homepage!.activeSearch.order = Models.Enums.ProjectOrder.LastOpenedDesc;
                break;

            case 3: // Collections
                homepage!.activeSearch.collections = [subArg!.Value];
                break;

            case 4: // Tags
                homepage!.activeSearch.tags = [subArg!.Value];
                break;
        }

        for (int i = 0; i < cont_Entries.Children.Count; i++)
            (cont_Entries.Children[i] as ISidebarControl)?.setSelected = i == id;

        await homepage!.SearchCards(false);
    }

    private void TaggingChange(int? projId, string msg)
    {
        if (projId.HasValue)
            return;

        switch (msg)
        {
            case nameof(ITaggingLogic.DeleteCollection):
            case nameof(ITaggingLogic.CreateOrUpdateCollection):
                DrawCollections().Wrap();
                break;

            case nameof(ITaggingLogic.DeleteTag):
            case nameof(ITaggingLogic.CreateOrUpdateTag):
                DrawTags().Wrap();
                break;
        }
    }

    private async Task DrawTags() => await entry_Tags.Init((cId) => UpdateSelection(4, cId), DependencyManager.GetService<ITaggingLogic>()!.GetTags);
    private async Task DrawCollections() => await entry_Cols.Init((cId) => UpdateSelection(3, cId), DependencyManager.GetService<ITaggingLogic>()!.GetCollections);

    private async Task DrawStorageUsage()
    {
        (ProjectInfo[] info, _) = await DependencyManager.GetService<IProjectLogic>()!.Search(new ProjectSearch()
        {
            order = ProjectOrder.SizeDesc
        });

        Dictionary<DriveInfo, List<ProjectInfo>> projectPerDrive = new();
        DriveInfo[] drives = DriveInfo.GetDrives();

        foreach (ProjectInfo proj in info)
        {
            DriveInfo? mostComplexMatch = drives.Where(d => proj.directory.StartsWith(d.Name))
                .OrderByDescending(d => d.Name.Length)
                .FirstOrDefault();

            if (mostComplexMatch == null)
                continue;

            if (projectPerDrive.ContainsKey(mostComplexMatch))
                projectPerDrive[mostComplexMatch].Add(proj);
            else
                projectPerDrive[mostComplexMatch] = new() { proj };
        }

        storageUsage.Draw(projectPerDrive, (ui, _, dat) =>
        {
            ui.DrawSlices(dat.Key, dat.Value.OrderByDescending(v => v.size)).Wrap();
        });
    }
}