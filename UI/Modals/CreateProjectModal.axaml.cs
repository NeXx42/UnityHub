using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;

namespace UI.Modals;

public partial class CreateProjectModal : UserControl, IModal
{
    private TaskCompletionSource? task;
    private string[] installedVersions = [];

    public CreateProjectModal()
    {
        InitializeComponent();

        btn_Browse.RegisterClick(UpdateLocation);
        btn_Create.RegisterClick(CreateProject);
    }

    public ModalContainer setContainer { set => _ = value; }

    public Task Show()
    {
        task?.SetCanceled();
        task = new TaskCompletionSource();

        Draw().Wrap();
        return task.Task;
    }

    private async Task Draw()
    {
        installedVersions = (await DependencyManager.GetService<IEditorLogic>()!.GetInstalledEditorVersions()).OrderByDescending(v => v).ToArray();
        inp_Versions.ItemsSource = installedVersions;
    }

    private async Task UpdateLocation()
    {
        string? path = await MainWindow.OpenFolderDialog("Location");

        if (string.IsNullOrEmpty(path))
            return;

        inp_Location.Text = path;
    }

    private async Task CreateProject()
    {
        string? projName = inp_Name.Text;
        string? loc = inp_Location.Text;
        string version = installedVersions[inp_Versions.SelectedIndex];

        if (string.IsNullOrEmpty(loc) || string.IsNullOrEmpty(projName))
            return;

        ProjectCreationInfo info = new ProjectCreationInfo()
        {
            info = new ProjectInfo()
            {
                id = -1,
                name = projName,
                version = version,
                directory = Path.Combine(loc, projName)
            }
        };

        IEditorLogic editorLogic = DependencyManager.GetService<IEditorLogic>()!;
        IProjectLogic projectLogic = DependencyManager.GetService<IProjectLogic>()!;

        await editorLogic.CreateProject(info);
        ProjectInfo? newInfo = await projectLogic.VerifyProjectPrimative(info.info);

        if (newInfo == null)
        {
            await DependencyManager.ui!.ShowMessageBox("Failed to create", "Failed to create project");
            return;
        }

        await projectLogic.UploadCardsPrimitive([newInfo]);
        task?.SetResult();
    }
}