using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Helpers;
using UI.Interfaces;

namespace UI.Popups;

public partial class Popup_Filter : UserControl, IPopup
{
    private TaskCompletionSource? openTask;
    private ReusableList<Popup_FilterGroup> filters;

    private ProjectSearch? activeSearch;
    private Func<Task>? search;

    private int? selectedGroup
    {
        set
        {
            m_selectedGroup = value;

            for (int i = 0; i < filters.getElementCount; i++)
                filters[i].toggle = i == m_selectedGroup;
        }
    }
    private int? m_selectedGroup;

    public Popup_Filter()
    {
        InitializeComponent();
        filters = new ReusableList<Popup_FilterGroup>(cont);
    }

    public void Init(ProjectSearch activeFilter, Func<Task> search)
    {
        this.search = search;
        this.activeSearch = activeFilter;
    }

    public Task Show()
    {
        openTask?.SetCanceled();
        openTask = new TaskCompletionSource();

        Func<Popup_FilterGroup, int, Task>[] groups = [
            LoadTags,
            LoadCollections,
            LoadVersions,
        ];

        filters.Draw(groups, (ui, pos, caller) => caller(ui, pos).Wrap());
        return openTask.Task;
    }

    private async Task LoadTags(Popup_FilterGroup ui, int pos)
    {
        TagData[] tags = await DependencyManager.GetService<ITaggingLogic>()!.GetTags();
        List<int> selectedOptions = new List<int>();

        if (activeSearch?.tags != null)
            foreach (int tagId in activeSearch.tags)
            {
                for (int i = 0; i < tags.Length; i++)
                    if (tags[i].collectionId == tagId)
                        selectedOptions.Add(i);
            }

        ui.Init("Tags", tags.Select(t => t.collectionName), selectedOptions, true, () => selectedGroup = pos, OnUpdateSelection);

        async Task OnUpdateSelection(IEnumerable<int> selectedOptions)
        {
            activeSearch?.tags = selectedOptions.Select(pos => tags[pos].collectionId);
            await (search?.Invoke() ?? Task.CompletedTask);
        }
    }

    private async Task LoadCollections(Popup_FilterGroup ui, int pos)
    {
        CollectionData[] cols = await DependencyManager.GetService<ITaggingLogic>()!.GetCollections();
        List<int> selectedOptions = new List<int>();

        if (activeSearch?.collections != null)
            foreach (int colId in activeSearch.collections)
            {
                for (int i = 0; i < cols.Length; i++)
                    if (cols[i].collectionId == colId)
                        selectedOptions.Add(i);
            }

        ui.Init("Collections", cols.Select(t => t.collectionName), selectedOptions, true, () => selectedGroup = pos, OnUpdateSelection);

        async Task OnUpdateSelection(IEnumerable<int> selectedOptions)
        {
            activeSearch?.collections = selectedOptions.Select(pos => cols[pos].collectionId);
            await (search?.Invoke() ?? Task.CompletedTask);
        }
    }

    private async Task LoadVersions(Popup_FilterGroup ui, int pos)
    {
        List<string> individualVersions = (await DependencyManager.GetService<IEditorLogic>()!.GetInstalledEditorVersions()).ToList();
        individualVersions.AddRange(await DependencyManager.GetService<IProjectLogic>()!.GetProjectVersions());
        individualVersions = individualVersions.Distinct().OrderDescending().ToList();

        string[] allOptions = ["Any", "Installed", .. individualVersions];
        int existingOption = activeSearch?.versions?.Count() ?? 0;

        if (existingOption == 1)
        {
            existingOption = allOptions.IndexOf(activeSearch!.versions.ElementAt(0));
        }
        else if (existingOption > 1)
        {
            existingOption = 1; // if multiple in the filter its assumed to be filtering on installed
        }

        ui.Init("Version", allOptions, [existingOption], false, () => selectedGroup = pos, OnUpdateSelection);

        async Task OnUpdateSelection(IEnumerable<int> selectedOptions)
        {
            int? option = selectedOptions.FirstOrDefault();

            switch (option)
            {
                case null:
                case 0:
                    activeSearch?.versions = [];
                    break;

                case 1:
                    activeSearch?.versions = await DependencyManager.GetService<IEditorLogic>()!.GetInstalledEditorVersions();
                    break;

                default:
                    activeSearch?.versions = [allOptions[option.Value]]; // first two options are default filters
                    break;
            }

            await (search?.Invoke() ?? Task.CompletedTask);
        }
    }
}