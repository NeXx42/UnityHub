using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Logic;
using Models.Data;
using Models.Helpers;
using Models.Interfaces;
using UI;

namespace UnityHub.UI.Modals.Startup;

public partial class Startup_FirstSetup : UserControl, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged;

    private TaskCompletionSource launchTask;

    public string? selectedEditorFolder { get; private set; }
    public string? selectedProjectFolder { get; private set; }

    public Startup_FirstSetup()
    {
        InitializeComponent();

        lbl_EditorCount.Content = "-";
        lbl_ProjectCount.Content = "-";

        btn_BrowseEditors.RegisterClick(BrowseEditors);
        btn_BrowseProjects.RegisterClick(BrowseProjects);

        btn_Save.RegisterClick(Save);

        launchTask = new TaskCompletionSource();
    }

    public Task Handle() => launchTask.Task;

    private async Task BrowseProjects()
    {
        selectedProjectFolder = await MainWindow.OpenFolderDialog("Projects");

        if (string.IsNullOrEmpty(selectedProjectFolder))
            return;

        int count = Directory.GetDirectories(selectedProjectFolder).Length;
        lbl_ProjectCount.Content = count;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }

    private async Task BrowseEditors()
    {
        selectedEditorFolder = await MainWindow.OpenFolderDialog("Editors");

        if (string.IsNullOrEmpty(selectedEditorFolder))
            return;

        int count = Directory.GetDirectories(selectedEditorFolder).Length;
        lbl_EditorCount.Content = count;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }

    private async Task Save()
    {
        if (!string.IsNullOrEmpty(selectedEditorFolder))
        {
            await DependencyManager.GetService<IConfigLogic>()!.Set(Models.Enums.ConfigEntry.EditorPath, (string[])[selectedEditorFolder], true);
        }

        if (!string.IsNullOrEmpty(selectedProjectFolder))
        {
            IProjectLogic projectLogic = DependencyManager.GetService<IProjectLogic>()!;

            var projects = await projectLogic.VerifyProjectPrimative(Directory.GetDirectories(selectedProjectFolder));

            if (projects.Length > 0)
            {
                List<ProjectInfo> successfulProjects = new List<ProjectInfo>();

                await DependencyManager.ui!.LoadProgressive("Deriving", [
                    ..projects.Select(p => new LoadRequest(p.name, (c) => HandleDerive(p, c))),
                    new LoadRequest("Saving", (c) => projectLogic.UploadCardsPrimitive(successfulProjects))
                ]);

                async Task HandleDerive(ProjectInfo proj, CancellationToken token)
                {
                    ProjectInfo? res = await projectLogic.VerifyProjectPrimative(proj);

                    if (res != null)
                        successfulProjects.Add(res);
                }
            }
        }

        await Clear();
    }

    private async Task Clear()
    {
        await DependencyManager.GetService<IConfigLogic>()!.Set(Models.Enums.ConfigEntry.IsSetup, true, false);
        launchTask.SetResult();
    }
}