using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Interfaces;
using UI.Controls;

namespace UI.Modals;

public partial class CreateProjectModal : UserControl, IModal
{
    private string[] installedVersions = [];

    public CreateProjectModal()
    {
        InitializeComponent();

        btn_Browse.RegisterClick(UpdateLocation);
        btn_Create.RegisterClick(CreateProject);
    }

    public ModalContainer setContainer { set => _ = value; }

    public async Task Show()
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

        await DependencyManager.GetService<IEditorLogic>()!.CreateProject(projName, loc, version);
    }
}