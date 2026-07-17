using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Avalonia.Controls;
using Logic;
using Models.Data;
using Models.Helpers;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;
using UI.Modals;
using UI.Popups;

namespace UI.Pages.HomePage;

public partial class Page : UserControl, IPage, INotifyPropertyChanged
{
    private enum NewProjectOptions
    {
        Add_Existing,
        Add_Folder,
    }

    const int ItemsPerPage = 16;

    private int maxPages = 5;

    private int? currentSelectedCard;
    private Popup_Filter? filter;

    private IContentDisplay contentDisplayer;
    private ReusableList<ButtonWrapper> pageControls;
    private ReusableList<CollectionItem> activeFilters;

    public int projectCount;
    private string? lastTextFilter;
    private ProjectInfo[]? cardInfo;


    public string TotalProjectCountTxt => $"{projectCount} Project{(projectCount > 1 ? "s" : "")}";
    public ProjectSearch activeSearch { private set; get; }

    public new event PropertyChangedEventHandler? PropertyChanged;

    public Page()
    {
        InitializeComponent();

        contentDisplayer = new ContentDisplay_List(grid_Content);
        activeFilters = new ReusableList<CollectionItem>(cont_Filters);
        pageControls = new ReusableList<ButtonWrapper>(cont_PageControls);

        btn_NewProject.RegisterOptions(((NewProjectOptions[])System.Enum.GetValues(typeof(NewProjectOptions))).Select(s => s.GetDisplayName()), SelectNewProjectOption);
        btn_NewProject.RegisterClick(CreateNewProject);

        activeSearch = new ProjectSearch()
        {
            page = 0,
            take = ItemsPerPage
        };

        filter = new Popup_Filter();
        filter.Init(activeSearch, SearchCards);

        btn_Filters.RegisterPopup(filter);
        inp_Text.TextChanged += (_, __) => UpdateTextFilter().Wrap();

        DependencyManager.GetService<IProjectLogic>()!.RegisterCallback(ProjectLogicCallback);
    }


    public async Task<Control> Show()
    {
        IsVisible = true;

        Sidebar sidebar = new Sidebar();
        await sidebar.Init(this);

        return sidebar;
    }

    public Task Close()
    {
        IsVisible = false;
        return Task.CompletedTask;
    }

    public async Task SearchCards()
    {
        (cardInfo, projectCount) = await DependencyManager.GetService<IProjectLogic>()!.Search(activeSearch);
        maxPages = (int)Math.Ceiling(projectCount / (float)ItemsPerPage);

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalProjectCountTxt)));

        await contentDisplayer.Draw(cardInfo, SelectCard);
        await RedrawFilterList();
        RedrawPageControls();
    }

    private async Task SelectCard(int cardPos)
    {
        currentSelectedCard = cardPos;
        contentDisplayer.UpdateSelection(cardPos);

        await control_MoreInfo.Show(cardInfo![cardPos].id);
    }

    private async Task CreateNewProject()
    {
        CreateProjectModal proj = MainWindow.ShowModal<CreateProjectModal>(out _);
        await proj.Show();
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
            ProjectInfo[] cards = await DependencyManager.GetService<IProjectLogic>()!.VerifyProjectPrimative(toUpload);

            // show popup to confirm each card (this code would be in there)
            await DependencyManager.GetService<IProjectLogic>()!.UploadCardsPrimitive(cards);
        }
    }

    private async Task UpdatePage(int to)
    {
        activeSearch.page = to;
        await SearchCards();
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

    private async Task RedrawFilterList()
    {
        ITaggingLogic tagService = DependencyManager.GetService<ITaggingLogic>()!;

        List<CollectionData> filters = new List<CollectionData>();
        filters.AddRange(await tagService.MapCollections(activeSearch.collections ?? []));
        filters.AddRange(await tagService.MapTags(activeSearch.tags ?? []));
        filters.AddRange(activeSearch.versions.Select(v => new CollectionData()
        {
            collectionId = -1,
            collectionName = v,
            type = "version"
        }));

        activeFilters.Draw(filters, (ui, _, dat) => ui.Init(dat, () => RemoveFilter(dat), () => RemoveFilter(dat)));

        Task RemoveFilter(CollectionData data)
        {
            switch (data.type.ToLower())
            {
                case "version":
                    break;

                case "tag":
                    break;

                case "collection":
                    break;
            }

            return Task.CompletedTask;
        }
    }

    private void ProjectLogicCallback(string name)
    {
        switch (name)
        {
            case nameof(IProjectLogic.DeleteCard):
            case nameof(IProjectLogic.UploadCardsPrimitive):
                SearchCards().Wrap();
                break;
        }
    }

    private async Task UpdateTextFilter()
    {
        string txt = inp_Text.Text ?? "";

        if (txt.Equals(lastTextFilter, StringComparison.CurrentCulture))
            return;

        lastTextFilter = txt;
        activeSearch.text = lastTextFilter;

        await SearchCards();
    }

    interface IContentDisplay
    {
        public Task Draw(ProjectInfo[] cards, Func<int, Task> onSelect);
        public void UpdateSelection(int to);
    }

    abstract class ContentDisplay_Base<T> : IContentDisplay where T : UserControl
    {
        protected ReusableList<T> cards;

        public ContentDisplay_Base(ScrollViewer scroller)
        {
            Panel container = GetWrapper();

            scroller.Content = container;
            cards = new ReusableList<T>(container);
        }


        public abstract Task Draw(ProjectInfo[] cardInfo, Func<int, Task> onSelect);


        public void UpdateSelection(int to)
        {
            for (int i = 0; i < cards.getElementCount; i++)
                ToggleElementSelection(cards[i], i == to);
        }

        protected abstract Panel GetWrapper();
        protected abstract void ToggleElementSelection(T element, bool to);
    }

    class ContentDisplay_Grid : ContentDisplay_Base<ImageCard>
    {
        public ContentDisplay_Grid(ScrollViewer scroller) : base(scroller)
        {

        }

        protected override Panel GetWrapper()
        {
            WrapPanel container = new WrapPanel();
            container.ItemSpacing = 5;
            container.LineSpacing = 5;

            return container;
        }

        public override async Task Draw(ProjectInfo[] cardInfo, Func<int, Task> onSelect)
        {
            await cards.DrawWhenAll(cardInfo, (c, i, dat) => c.Draw(dat, i, onSelect));
        }

        protected override void ToggleElementSelection(ImageCard element, bool to) => element.ToggleSelection(to);
    }

    class ContentDisplay_List : ContentDisplay_Base<ListCard>
    {
        public ContentDisplay_List(ScrollViewer scroller) : base(scroller)
        {

        }

        protected override Panel GetWrapper()
        {
            StackPanel container = new StackPanel();
            container.Spacing = 5;
            container.Orientation = Avalonia.Layout.Orientation.Vertical;

            return container;
        }

        public override async Task Draw(ProjectInfo[] cardInfo, Func<int, Task> onSelect)
        {
            await cards.DrawWhenAll(cardInfo, (c, i, dat) => c.Draw(dat, i, onSelect));
        }

        protected override void ToggleElementSelection(ListCard element, bool to) => element.ToggleSelection(to);
    }
}