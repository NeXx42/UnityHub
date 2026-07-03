using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Logic;
using Models.Data;
using Models.Helpers;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;
using UI.Modals;

namespace UI.Pages.HomePage;

public partial class Page : UserControl
{
    private enum NewProjectOptions
    {
        Add_Existing,
        Add_Folder,
    }

    const int ItemsPerPage = 16;

    private int maxPages = 5;

    private int? currentSelectedCard;

    private ReusableList<ImageCard> cards;
    private ReusableList<ButtonWrapper> pageControls;

    private ProjectSearch activeSearch;
    private ProjectCard[]? cardInfo;

    public Page()
    {
        InitializeComponent();

        cards = new ReusableList<ImageCard>(grid_Content);
        pageControls = new ReusableList<ButtonWrapper>(cont_PageControls);

        btn_NewProject.RegisterOptions(((NewProjectOptions[])System.Enum.GetValues(typeof(NewProjectOptions))).Select(s => s.GetDisplayName()), SelectNewProjectOption);
        btn_NewProject.RegisterClick(CreateNewProject);

        activeSearch = new ProjectSearch()
        {
            page = 0
        };
    }

    public async Task SearchCards(ProjectSearch search)
    {
        activeSearch = search;
        search.take = ItemsPerPage;

        (cardInfo, int totalCards) = await DependencyManager.GetService<IProjectLogic>()!.Search(activeSearch);
        maxPages = (int)Math.Ceiling(totalCards / (float)ItemsPerPage);

        cards.Draw(cardInfo, (c, i, dat) => _ = c.Draw(dat, i, SelectCard));
        RedrawPageControls();
    }

    private async Task SelectCard(int cardPos)
    {
        if (currentSelectedCard.HasValue)
            cards[currentSelectedCard.Value].ToggleSelection(false);

        currentSelectedCard = cardPos;
        cards[currentSelectedCard.Value].ToggleSelection(false);

        await control_MoreInfo.Show(cardInfo![cardPos].id);
    }

    private async Task CreateNewProject()
    {
        MainWindow.ShowModal<CreateProjectModal>(out _);
    }

    private async Task SelectNewProjectOption(int id)
    {
        switch ((NewProjectOptions)id)
        {
            case NewProjectOptions.Add_Folder:
                string? root = await MainWindow.OpenFolderDialog("Select Folder");

                if (string.IsNullOrEmpty(root))
                    return;

                string[] dirs = Directory.GetDirectories(root);
                await AttemptUploadOfDirectories(dirs);
                break;

            case NewProjectOptions.Add_Existing:
                string[]? folders = await MainWindow.OpenFoldersDialog("Select Folder(s)");

                if ((folders?.Length ?? 0) == 0)
                    return;

                await AttemptUploadOfDirectories(folders!);
                break;
        }

        async Task AttemptUploadOfDirectories(string[] toUpload)
        {
            ProjectInfo[] cards = await DependencyManager.GetService<IProjectLogic>()!.TryToUpload(toUpload);

            // show popup to confirm each card (this code would be in there)
            await DependencyManager.GetService<IProjectLogic>()!.UploadCardsPrimitive(cards);
        }
    }

    private async Task UpdatePage(int to)
    {
        activeSearch.page = to;
        await SearchCards(activeSearch);
    }

    private void RedrawPageControls()
    {
        const int MaxPageDistance = 4;
        List<int> pageOptions = new List<int>();

        for (int i = activeSearch.page - MaxPageDistance; i < activeSearch.page + MaxPageDistance; i++)
        {
            if (i >= 0 && i < maxPages)
                pageOptions.Add(i);
        }

        pageControls.Draw(pageOptions, (lbl, _, dat) =>
        {
            lbl.Label = (dat + 1).ToString();
            lbl.RegisterClick(() => UpdatePage(dat));

            if (dat == activeSearch.page)
                lbl.Classes.Add("Selected");
            else
                lbl.Classes.Remove("Selected");
        });
    }
}