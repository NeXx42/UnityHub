using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Logic;
using Models.Data;
using Models.Enums;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;

namespace UI.Modals;

public partial class CreateProjectModal : UserControl, IModal
{
    private TaskCompletionSource? task;
    private EditorInstallInfo[] installedVersions = [];

    private Dictionary<string, string> selectedPackages = new();
    private ReusableList<CreateProjectModal_Package> packageList;

    public CreateProjectModal()
    {
        InitializeComponent();

        btn_Browse.RegisterClick(UpdateLocation);
        btn_Create.RegisterClick(CreateProject);

        inp_Pipeline.SelectionChanged += (_, __) => UpdateSelectedPipeline();
        inp_Versions.SelectionChanged += (_, __) => UpdateSelectedVersion().Wrap();

        packageList = new ReusableList<CreateProjectModal_Package>(cont_Packages);
    }

    public bool canDismiss => true;
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
        string? lastSaveLocation = await DependencyManager.GetService<IConfigLogic>()!.Get(ConfigEntry.LastSaveLocation, string.Empty);

        if (!string.IsNullOrEmpty(lastSaveLocation) && Directory.Exists(lastSaveLocation))
            inp_Location.Text = lastSaveLocation;

        installedVersions = (await DependencyManager.GetService<IEditorLogic>()!.GetEditorMetadataForDownloadedVersions(System.Threading.CancellationToken.None)).OrderByDescending(v => v.versionName).ToArray();

        inp_Versions.ItemsSource = installedVersions.Select(v => v.versionName);
        inp_Versions.SelectedIndex = 0;

        await UpdateSelectedVersion();
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
        EditorInstallInfo version = installedVersions[inp_Versions.SelectedIndex];

        if (string.IsNullOrEmpty(loc) || string.IsNullOrEmpty(projName))
            return;

        ProjectCreationInfo info = new ProjectCreationInfo()
        {
            packages = selectedPackages,
            info = new ProjectInfo()
            {
                id = -1,
                name = projName,
                version = version.versionName,
                directory = Path.Combine(loc, projName),
                collectionId = (int)DefaultCollectionIds.InDevelopment
            }
        };

        IEditorLogic editorLogic = DependencyManager.GetService<IEditorLogic>()!;
        IProjectLogic projectLogic = DependencyManager.GetService<IProjectLogic>()!;

        if (!await editorLogic.CreateProject(info))
        {
            return;
        }

        ProjectInfo? newInfo = await projectLogic.VerifyProjectPrimative(info.info);

        if (newInfo == null)
        {
            await DependencyManager.ui!.ShowMessageBox("Failed to create", "Failed to create project");
            return;
        }

        await DependencyManager.GetService<IConfigLogic>()!.Set(ConfigEntry.LastSaveLocation, loc, true);
        await projectLogic.UploadCardsPrimitive([newInfo]);
        await editorLogic.LaunchProject(newInfo);

        task?.SetResult();
    }

    private async Task UpdateSelectedVersion()
    {
        if (inp_Versions.SelectedIndex < 0 || inp_Versions.SelectedIndex >= installedVersions.Length)
            return;

        EditorInstallInfo version = installedVersions[inp_Versions.SelectedIndex];

        packageList.Draw(version.builtInPackages.OrderByDescending(p => p.displayName), (ui, _, dat) =>
        {
            ui.Draw(dat);
        });

        UpdateSelectedPipeline();
    }

    private void UpdateSelectedPipeline()
    {
        EditorInstallInfo version = installedVersions[inp_Versions.SelectedIndex];
        selectedPackages.Clear();

        switch (inp_Pipeline.SelectedIndex)
        {
            case 0: // urp
                GetPackageAndDependencies("com.unity.render-pipelines.universal", ref selectedPackages);
                break;

            case 1: // hdrp
                GetPackageAndDependencies("com.unity.render-pipelines.high-definition", ref selectedPackages);
                break;

            default: // built in
                break;
        }

        for (int i = 0; i < packageList.getElementCount; i++)
            packageList[i].UpdateInPackageList(ref selectedPackages);

        void GetPackageAndDependencies(string rootPkg, ref Dictionary<string, string> intoCollection)
        {
            var pipelinePkg = version.builtInPackages.FirstOrDefault(v => v.name.Equals(rootPkg));

            if (!string.IsNullOrEmpty(pipelinePkg.name))
            {
                intoCollection = new Dictionary<string, string>()
                {
                    {pipelinePkg.name, pipelinePkg.version}
                };

                if (pipelinePkg.dependencies != null)
                    foreach (var dep in pipelinePkg.dependencies)
                        intoCollection.Add(dep.Key, dep.Value);
            }

        }
    }
}