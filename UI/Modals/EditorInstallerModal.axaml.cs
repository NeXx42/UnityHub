using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Enums;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;

namespace UI.Modals;

public partial class EditorInstallerModal : UserControl, IModal
{
    private TaskCompletionSource? modalTask;

    private EditorFilterType selectedFilter;

    private ReusableList<ButtonWrapper> menuOptionsList;
    private ReusableList<EditorInstallerModal_Entry> entryList;

    private int currentPage;
    private int maxPages;

    private const int pageTake = 10;

    public EditorInstallerModal()
    {
        InitializeComponent();

        menuOptionsList = new ReusableList<ButtonWrapper>(cont_Types);
        entryList = new ReusableList<EditorInstallerModal_Entry>(entries);

        menuOptionsList.Draw(System.Enum.GetValues<EditorFilterType>(), (ui, _, dat) =>
        {
            ui.Label = dat.ToString();
            ui.RegisterClick(() => UpdateSelectedEditorType(dat));
        });
        btn_Search.RegisterClick(() => UpdateSelectedEditorType(EditorFilterType.Archive));

        btn_PrevPage.RegisterClick(() => UpdatePage(-1));
        btn_NextPage.RegisterClick(() => UpdatePage(1));
    }

    public ModalContainer setContainer { set => _ = value; }

    public Task Open()
    {
        modalTask = new TaskCompletionSource();
        UpdateSelectedEditorType(EditorFilterType.LTS).Wrap();

        return modalTask.Task;
    }

    private async Task StartInstall(EditorInfo version)
    {
        EditorModuleInstallerModal modal = MainWindow.ShowModal<EditorModuleInstallerModal>(out int pos);
        await modal.Show(version);
        await MainWindow.CloseModal(pos);
    }

    private async Task UpdateSelectedEditorType(EditorFilterType type)
    {
        if (selectedFilter != type)
        {
            currentPage = 0;
            selectedFilter = type;
        }

        EditorFilterType[] filterTypes = System.Enum.GetValues<EditorFilterType>();

        for (int i = 0; i < filterTypes.Length; i++)
            if (filterTypes[i] == type)
                menuOptionsList[i].Classes.Add("Primary");
            else
                menuOptionsList[i].Classes.Remove("Primary");

        EditorInfo[]? editors = await loadingBoundary.Load(GetPotentialInstalls);
        entryList.Draw(editors ?? [], (ui, _, dat) => ui.Draw(dat, StartInstall));

        async Task<EditorInfo[]> GetPotentialInstalls()
        {
            (EditorInfo[] info, int resultCount) = await DependencyManager.GetService<IEditorLogic>()!.GetEditorDownloads(selectedFilter, inp_VersionFilter.Text, currentPage, pageTake);
            maxPages = (int)Math.Ceiling(resultCount / (float)pageTake);

            return info;
        }
    }

    private async Task UpdatePage(int delta)
    {
        int newPage = currentPage + delta;
        newPage = Math.Max(Math.Min(newPage, maxPages), 0);

        if (newPage == currentPage)
            return;

        await UpdateSelectedEditorType(selectedFilter);
    }
}