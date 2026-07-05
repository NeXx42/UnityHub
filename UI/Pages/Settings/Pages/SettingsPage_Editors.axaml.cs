using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;

namespace UI.Pages.Settings.Pages;

public partial class SettingsPage_Editors : UserControl, ISettingsPage
{
    private ReusableList<ButtonWrapper> editorLocations;
    private List<string>? installLocations;

    public SettingsPage_Editors()
    {
        InitializeComponent();

        editorLocations = new ReusableList<ButtonWrapper>(cont_Locations);
        btn_AddLocation.RegisterClick(() => UpdateLocation(null));
    }

    public UserControl getControl => this;

    public async Task OnOpen()
    {
        await RedrawLocations();
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
}