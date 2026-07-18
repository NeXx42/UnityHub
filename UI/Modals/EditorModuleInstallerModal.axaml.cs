using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;

namespace UI.Modals;

public partial class EditorModuleInstallerModal : UserControl, IModal
{
    private TaskCompletionSource? modalTask;

    private EditorInfo? selectedVersion;
    private string[]? paths;

    public EditorModuleInstallerModal()
    {
        InitializeComponent();

        btn_Install.RegisterClick(Install);
    }

    public bool canDismiss => true;
    public ModalContainer setContainer { set => _ = value; }

    public Task Show(EditorInfo info)
    {
        modalTask = new TaskCompletionSource();
        this.DataContext = info;

        selectedVersion = info;
        LoadModal().Wrap();

        return modalTask.Task;
    }

    private async Task LoadModal()
    {
        paths = (await DependencyManager.GetService<IEditorLogic>()!.GetEditorLocations(true)).OrderByDescending(x => x).ToArray();
        inp_Locations.ItemsSource = paths;
    }

    private async Task Install()
    {
        string? selectedPath = inp_Locations.SelectedValue?.ToString();

        if (string.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath) || selectedVersion == null)
            return;

        int download = 0;

        for (int i = 0; i < selectedVersion.downloads.Length; i++)
            if (selectedVersion.downloads[i].platform!.Equals("linux", System.StringComparison.CurrentCultureIgnoreCase))
            {
                download = i;
                break;
            }

        await DependencyManager.GetService<IEditorLogic>()!.InstallEditor(selectedVersion, download, selectedPath);
    }
}