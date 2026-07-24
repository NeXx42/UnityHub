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

public partial class MoveProject_Modal : UserControl, IModal
{
    private TaskCompletionSource? modalTask;
    private ProjectInfo? info;

    public MoveProject_Modal()
    {
        InitializeComponent();

        btn_Browse.RegisterClick(Browse);
        btn_Move.RegisterClick(Move);
    }

    public ModalContainer setContainer { set => _ = value; }
    public bool canDismiss => true;

    public Task Open(ProjectInfo info)
    {
        modalTask = new TaskCompletionSource();

        this.info = info;
        inp_Path.Text = new DirectoryInfo(info.directory).Parent?.FullName;
        inp_FolderName.Text = info.name;

        return modalTask.Task;
    }

    private async Task Browse()
    {
        string? res = await MainWindow.OpenFolderDialog("New Directory");
        inp_Path.Text = res;
    }

    private async Task Move()
    {
        if (info == null)
            return;

        string? path = Path.Combine(inp_Path.Text ?? "", inp_FolderName.Text ?? "");

        if (string.IsNullOrEmpty(path))
        {
            await DependencyManager.ui!.ShowMessageBox("Invalid path", $"Path ({path}) is invalid, please select another.");
            return;
        }

        if (await DependencyManager.ui!.ShowConfirmationBox("Are you sure?", $"Move project from '{info.directory}' to '{path}'", new ConfirmationButton("Cancel"), new ConfirmationButton("Move", true)) != 1)
            return;

        LoadRequest[] reqs;

        try
        {
            reqs = DependencyManager.GetService<IProjectLogic>()!.MoveProject(info, path);
        }
        catch (Exception e)
        {
            await DependencyManager.ui.ShowMessageBox(e);
            return;
        }

        Exception? error = await DependencyManager.ui!.LoadProgressive("Moving", reqs);

        if (error != null)
            await DependencyManager.ui.ShowMessageBox(error);

        modalTask?.SetResult();
    }
}