using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Microsoft.VisualBasic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;
using UI.Modals;

namespace UI.Modals;

public partial class EditorManagerModal : UserControl, IModal
{
    private ReusableList<EditorManager_ModuleGroup> moduleList;
    private ReusableList<CollectionItem> tags;

    private TaskCompletionSource? openTask;
    private EditorInfo? editorInfo;

    public EditorManagerModal()
    {
        InitializeComponent();

        tags = new ReusableList<CollectionItem>(cont_Tags);
        moduleList = new ReusableList<EditorManager_ModuleGroup>(cont_Modules);

        btn_Install.RegisterClick(Install);
    }

    public ModalContainer setContainer { set => _ = value; }
    public bool canDismiss => true;

    public Task Open<T>(T info) where T : EditorInfo
    {
        openTask = new TaskCompletionSource();

        this.DataContext = info;
        this.editorInfo = info;

        HashSet<string> installedModules = new();

        if (info is EditorInstallInfo installInfo)
        {
            installedModules = installInfo.installedPackages;

            inp_Path.ItemsSource = (string[])[installInfo.installLocation];
            inp_Path.SelectedIndex = 0;

            inp_Path.IsEnabled = false;
        }
        else
        {
            FindRemotePaths().Wrap();
            inp_Path.IsEnabled = true;
        }

        Dictionary<string, EditorInfo.Download.Module[]> modules = (info.download?.modules ?? [])
            .GroupBy(m => m.category ?? "", m => m)
            .ToDictionary(m => m.Key, m => m.ToArray());

        moduleList.Draw(modules, (ui, pos, dat) => ui.Draw(dat.Key, dat.Value, installedModules));
        tags.Draw(info.CreateTags(), (ui, _, dat) => ui.Init(dat));

        return openTask.Task;
    }

    private async Task FindRemotePaths()
    {
        string[] installPaths = (await DependencyManager.GetService<IEditorLogic>()!.GetEditorLocations(true)).OrderByDescending(x => x).ToArray();

        inp_Path.ItemsSource = installPaths;
        inp_Path.SelectedIndex = 0;
    }

    private async Task Install()
    {
        if (editorInfo == null)
            return;

        HashSet<string> desiredModules = new();

        foreach (EditorManager_ModuleGroup ui in moduleList)
            ui.AddSelectedModules(ref desiredModules);

        await DependencyManager.GetService<IEditorLogic>()!.InstallEditor(editorInfo, desiredModules, (string?)inp_Path.SelectedValue);
        openTask?.SetResult();
    }
}