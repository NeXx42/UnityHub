using System.Collections.Generic;
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
using UI.Modals;

namespace UI.Pages.Settings.Pages;

public partial class SettingsPage_Editors : UserControl, ISettingsPage
{
    private ReusableList<ButtonWrapper> editorLocations;
    private ReusableList<SettingsPage_Editors_InstalledVersion> editorInstalls;

    private List<string>? installLocations;

    public SettingsPage_Editors()
    {
        InitializeComponent();

        editorLocations = new ReusableList<ButtonWrapper>(cont_Locations);
        editorInstalls = new ReusableList<SettingsPage_Editors_InstalledVersion>(cont_Versions);

        btn_AddLocation.RegisterClick(() => UpdateLocation(null));
        btn_InstallVersion.RegisterClick(InstallNewEditor);
    }

    public UserControl getControl => this;

    public async Task OnOpen()
    {
        await RedrawLocations();
        RedrawDownloaded().Wrap(); // can be slow, so let it load without blocking this
    }

    private async Task UpdateLocation(int? index)
    {
        string? newPath = await MainWindow.OpenFolderDialog("Installs location");

        if (string.IsNullOrEmpty(newPath) || !Directory.Exists(newPath))
            return;

        if (index.HasValue)
            installLocations![index.Value] = newPath;
        else
            installLocations!.Add(newPath);

        await DependencyManager.GetService<IConfigLogic>()!.Set(Models.Enums.ConfigEntry.EditorPath, installLocations, true);
        await RedrawLocations();
    }

    private async Task RedrawLocations()
    {
        installLocations = await DependencyManager.GetService<IConfigLogic>()!.Get<List<string>>(Models.Enums.ConfigEntry.EditorPath, []);

        editorLocations.Draw(installLocations, (ui, pos, dat) =>
        {
            ui.Label = dat;
            ui.RegisterClick(() => UpdateLocation(pos));
        });
    }

    private async Task RedrawDownloaded()
    {
        IEditorLogic logic = DependencyManager.GetService<IEditorLogic>()!;

        EditorInstallInfo[] installed = await logic.GetInstalledEditorVersionsMoreInfo(System.Threading.CancellationToken.None);
        Dictionary<EditorInfo, DownloadStatus> downloading = logic.GetActiveInstalls();

        (EditorInfo, DownloadStatus?)[] installs = [.. installed.Select(i => (i, (DownloadStatus?)null)), .. downloading.Select(d => (d.Key, d.Value))];

        editorInstalls.Draw(installs, (ui, _, dat) => ui.Draw(dat.Item1, dat.Item2));
    }

    private async Task InstallNewEditor()
    {
        EditorInstallerModal modal = MainWindow.ShowModal<EditorInstallerModal>(out int pos);
        await modal.Open();

        await MainWindow.CloseModal(pos);
        await RedrawDownloaded();
    }
}