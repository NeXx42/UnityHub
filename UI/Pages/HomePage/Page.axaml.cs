using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Logic;
using Models.Data;
using Models.Enums;
using Models.Helpers;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;
using UI.Interfaces;
using UI.Modals;
using UI.Pages.HomePage.ContentDisplays;
using UI.Popups;

namespace UI.Pages.HomePage;

public interface IPlugin_HomePage : IFrontendPlugin
{
    public void RegisterLayout(List<Type> displays);
}

public partial class Page : UserControl, IPage, INotifyPropertyChanged
{
    public static FrontendPluginHandler<IPlugin_HomePage> plugins = new();

    private enum NewProjectOptions
    {
        Add_Existing,
        Add_Folder,
    }
    private int maxPages = 5;

    private int? currentSelectedCard;
    private Popup_Filter? filter;
    private Popup_Sort? sort;

    private List<IHomePageLayout> contentDisplayers;
    private ReusableList<CollectionItem> activeFilters;

    public int projectCount;
    private string? lastTextFilter;
    private ProjectInfo[]? cardInfo;

    public string TotalProjectCountTxt => $"{projectCount} Project{(projectCount > 1 ? "s" : "")}";
    public ProjectSearch activeSearch { private set; get; }

    private int selectedContentDisplayer = 0;
    public IHomePageLayout getCurrentContentDisplay => contentDisplayers[selectedContentDisplayer];

    public new event PropertyChangedEventHandler? PropertyChanged;

    public Page()
    {
        activeSearch = new ProjectSearch()
        {
            page = 0,
            take = 0
        };

        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        List<Type> layouts = new()
        {
            typeof(HomePageLayout_Grid),
            typeof(HomePageLayout_List),
            typeof(HomePageLayout_Table),
        };
        plugins.Execute(p => p.RegisterLayout(layouts));
        contentDisplayers = new List<IHomePageLayout>();

        foreach (var layout in layouts)
        {
            if (layout.IsAssignableTo(typeof(IHomePageLayout)))
            {
                int layoutId = contentDisplayers.Count;

                IHomePageLayout layoutControl = (IHomePageLayout)Activator.CreateInstance(layout, grid_Content, (int p) => UpdatePage(p).Wrap())!;
                contentDisplayers.Add(layoutControl);

                ButtonWrapper btn = layoutControl.CreateButton();
                btn.RegisterClick(() => UpdateLayout(layoutId));
                cont_Layouts.Children.Add(btn);

                layoutControl.ToggleVisibility(false);
            }
        }

        UpdateLayout(0).Wrap();

        activeFilters = new ReusableList<CollectionItem>(cont_Filters);

        btn_NewProject.RegisterOptions(((NewProjectOptions[])System.Enum.GetValues(typeof(NewProjectOptions))).Select(s => s.GetDisplayName()), SelectNewProjectOption);
        btn_NewProject.RegisterClick(CreateNewProject);

        filter = new Popup_Filter();
        filter.Init(activeSearch, () => SearchCards(false));
        btn_Filters.RegisterPopup(filter);

        sort = new Popup_Sort();
        sort.Draw(activeSearch, UpdateSort);
        btn_Sort.RegisterPopup(sort);

        inp_Text.TextChanged += (_, __) => UpdateTextFilter().Wrap();

        DependencyManager.GetService<IProjectLogic>()?.RegisterCallback(ProjectLogicCallback);
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

    public async Task SearchCards(bool isPageIncrement)
    {
        activeSearch.take = getCurrentContentDisplay.getTake;

        (cardInfo, projectCount) = await DependencyManager.GetService<IProjectLogic>()!.Search(activeSearch);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalProjectCountTxt)));

        await getCurrentContentDisplay.Draw(cardInfo, isPageIncrement, activeSearch.page, projectCount, SelectCard);
        await RedrawFilterList();
    }

    private async Task SelectCard(int cardPos)
    {
        currentSelectedCard = cardPos;
        getCurrentContentDisplay.UpdateSelection(cardPos);

        await control_MoreInfo.Show(cardInfo![cardPos].id);
    }

    private async Task CreateNewProject()
    {
        CreateProjectModal proj = MainWindow.ShowModal<CreateProjectModal>(out int id);
        await proj.Show();

        await MainWindow.CloseModal(id);
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
        await SearchCards(true);
    }

    private async Task RedrawFilterList()
    {
        btn_Sort.Label = FormatOrderName(activeSearch.order);

        ITaggingLogic tagService = DependencyManager.GetService<ITaggingLogic>()!;

        List<TagData> filters = new List<TagData>();
        filters.AddRange(await tagService.MapCollections(activeSearch.collections ?? []));
        filters.AddRange(await tagService.MapTags(activeSearch.tags ?? []));
        filters.AddRange(activeSearch.versions.Select(v => new SearchFilterCollectionStandIn()
        {
            collectionId = -1,
            collectionName = v,
            customType = "version"
        }));

        activeFilters.Draw(filters, (ui, _, dat) => ui.Init(dat, () => RemoveFilter(dat), () => RemoveFilter(dat)));

        Task RemoveFilter(TagData data)
        {
            if (data is CollectionData)
            {

            }
            else if (data is SearchFilterCollectionStandIn standIn)
            {
                switch (standIn.customType)
                {
                    case "version":

                        break;
                }
            }
            else
            {

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
                SearchCards(false).Wrap();
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

        await SearchCards(false);
    }

    private async Task UpdateSort(ProjectOrder sort)
    {
        activeSearch.order = sort;
        btn_Sort.Label = FormatOrderName(activeSearch.order);

        await SearchCards(false);
    }

    private async Task UpdateLayout(int to)
    {
        getCurrentContentDisplay.ToggleVisibility(false);
        selectedContentDisplayer = to;
        getCurrentContentDisplay.ToggleVisibility(true);

        await SearchCards(false);
    }

    class SearchFilterCollectionStandIn : TagData
    {
        public string customType = "";
    }

    private string FormatOrderName(ProjectOrder order)
    {
        StringBuilder sb = new StringBuilder();

        foreach (char c in order.ToString())
        {
            if (char.IsUpper(c))
                sb.Append(" ");

            sb.Append(c);
        }

        return sb.ToString().Substring(1);
    }
}