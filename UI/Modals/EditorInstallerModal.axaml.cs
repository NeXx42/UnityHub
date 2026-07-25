using System;
using System.Collections.Generic;
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
    private ReusableList<ButtonWrapper> pageControlList;
    private ReusableList<EditorInstallerModal_Entry> entryList;

    private int currentPage;
    private int maxPages;

    private const int pageTake = 10;

    public EditorInstallerModal()
    {
        InitializeComponent();

        menuOptionsList = new ReusableList<ButtonWrapper>(cont_Types);
        entryList = new ReusableList<EditorInstallerModal_Entry>(entries);
        pageControlList = new ReusableList<ButtonWrapper>(cont_PageControls);

        menuOptionsList.Draw(Enum.GetValues<EditorFilterType>(), (ui, _, dat) =>
        {
            ui.Label = dat.ToString();
            ui.RegisterClick(() => UpdateSelectedEditorType(dat, true));
        });
        btn_Search.RegisterClick(() => UpdateSelectedEditorType(EditorFilterType.Archive, true));
    }

    public bool canDismiss => true;
    public ModalContainer setContainer { set => _ = value; }

    public Task Open()
    {
        modalTask = new TaskCompletionSource();
        UpdateSelectedEditorType(EditorFilterType.LTS, true).Wrap();

        return modalTask.Task;
    }
    private async Task UpdateSelectedEditorType(EditorFilterType type, bool resetPage)
    {
        if (resetPage)
            currentPage = 0;

        selectedFilter = type;
        EditorFilterType[] filterTypes = System.Enum.GetValues<EditorFilterType>();

        for (int i = 0; i < filterTypes.Length; i++)
            if (filterTypes[i] == type)
                menuOptionsList[i].Classes.Add("Primary");
            else
                menuOptionsList[i].Classes.Remove("Primary");

        EditorInfo[]? editors = await loadingBoundary.Load(GetPotentialInstalls);
        entryList.Draw(editors ?? [], (ui, pos, dat) => ui.Draw(dat, pos, OpenInstallModal));

        async Task<EditorInfo[]> GetPotentialInstalls()
        {
            (EditorInfo[] info, int resultCount) = await DependencyManager.GetService<IEditorLogic>()!.SearchEditorDownloads(selectedFilter, inp_VersionFilter.Text, currentPage, pageTake);

            maxPages = (int)Math.Ceiling(resultCount / (float)pageTake);
            RedrawPageControls();

            return info;
        }

        async Task OpenInstallModal(EditorInfo info)
        {
            await MainWindow.ShowModalAndWait<EditorManagerModal>(async m =>
            {
                await m.Open(info);
            });
        }
    }

    private async Task UpdatePage(int to)
    {
        if (currentPage == to)
            return;

        currentPage = to;
        await UpdateSelectedEditorType(selectedFilter, false);
    }

    private void RedrawPageControls()
    {
        if (maxPages <= 1)
        {
            pageControlList.Clear();
            return;
        }

        const int MaxPageDistance = 4;
        List<int> pageOptions = new List<int>();

        for (int i = currentPage - MaxPageDistance; i < currentPage + MaxPageDistance; i++)
        {
            if (i >= 0 && i < maxPages)
                pageOptions.Add(i);
        }

        pageControlList.Draw(pageOptions, (lbl, _, dat) =>
        {
            lbl.Label = (dat + 1).ToString();
            lbl.RegisterClick(() => UpdatePage(dat));

            if (dat == currentPage)
                lbl.Classes.Add("Primary");
            else
                lbl.Classes.Remove("Primary");
        });
    }
}