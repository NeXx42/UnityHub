using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Helpers;
using Models.Interfaces;
using UI.Helpers;
using UI.Modals;
using UI.Pages.HomePage;

namespace UI.Pages.HomePage;

public partial class Page : UserControl
{
    private enum NewProjectOptions
    {
        Add_Existing,
        Add_Folder,
    }

    private int? currentSelectedCard;
    private ReusableList<ImageCard> cards;

    private ProjectCard[]? cardInfo;

    public Page()
    {
        InitializeComponent();

        cards = new ReusableList<ImageCard>(grid_Content);

        btn_NewProject.RegisterOptions(((NewProjectOptions[])System.Enum.GetValues(typeof(NewProjectOptions))).Select(s => s.GetDisplayName()), SelectNewProjectOption);
        btn_NewProject.RegisterClick(CreateNewProject);
    }

    public async Task Draw()
    {
        await SearchCards(new ProjectSearch()
        {
            take = 16,
        });
    }

    public async Task SearchCards(ProjectSearch search)
    {
        search.take = 16;

        cardInfo = await DependencyManager.GetService<IProjectLogic>()!.Search(search);
        cards.Draw(cardInfo, (c, i, dat) => _ = c.Draw(dat, i, SelectCard));
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
}