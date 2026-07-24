using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;

namespace UI.Modals;

public partial class DuplicateProject_Modal : UserControl, IModal
{
    private TaskCompletionSource? modalTask;
    private ProjectInfo? info;

    public DuplicateProject_Modal()
    {
        InitializeComponent();

        btn_Browse.RegisterClick(Browse);
        btn_Clone.RegisterClick(Clone);
    }

    public ModalContainer setContainer { set => _ = value; }
    public bool canDismiss => true;

    public Task Open(ProjectInfo info)
    {
        modalTask = new TaskCompletionSource();

        this.info = info;
        inp_Name.Text = $"{info.name} (Copy)";
        inp_Path.Text = new DirectoryInfo(info.directory).Parent?.FullName;
        inp_FolderName.Text = $"{info.name}_Copy";

        return modalTask.Task;
    }

    private async Task Browse()
    {
        string? res = await MainWindow.OpenFolderDialog("New Directory");
        inp_Path.Text = res;
    }

    private async Task Clone()
    {
        if (info == null)
            return;

        string path = Path.Combine(inp_Path.Text ?? "", inp_FolderName.Text ?? "");

        if (string.IsNullOrEmpty(path))
        {
            await DependencyManager.ui!.ShowMessageBox("Invalid path", $"Path ({path}) is invalid, please select another.");
            return;
        }

        string? name = inp_Name.Text;

        if (string.IsNullOrEmpty(name))
        {
            await DependencyManager.ui!.ShowMessageBox("Invalid name", $"Name ({path}) is invalid, please use another.");
            return;
        }

        if (await DependencyManager.ui!.ShowConfirmationBox("Are you sure?", $"Duplicate project into '{path}'", new ConfirmationButton("Cancel"), new ConfirmationButton("Clone", true)) != 1)
            return;

        Exception? error = await DependencyManager.ui!.LoadProgressive("Duplicating", DependencyManager.GetService<IProjectLogic>()!.DuplicateProject(info, name, path));

        if (error != null)
            await DependencyManager.ui.ShowMessageBox(error);

        modalTask?.SetResult();
    }
}