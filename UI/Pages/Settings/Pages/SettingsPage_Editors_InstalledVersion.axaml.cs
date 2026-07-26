using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;
using UI.Modals;
using UI.Popups;

namespace UI.Pages.Settings.Pages;

public partial class SettingsPage_Editors_InstalledVersion : UserControl, INotifyPropertyChanged
{
    public string ProductName { get; set; } = "";
    public string VersionNumber { get; set; } = "";
    public string InstallLocation { get; set; } = "";

    private ReusableList<CollectionItem> tagLines;
    private ReusableList<Border> platforms;

    private EditorInfo? info;
    private Func<Task>? redrawRequest;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public SettingsPage_Editors_InstalledVersion()
    {
        InitializeComponent();

        tagLines = new ReusableList<CollectionItem>(cont_Tags);
        platforms = new ReusableList<Border>(cont_Platforms, CreateTag);

        Border CreateTag()
        {
            return new Border()
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),

                Child = new Label
                {
                    FontSize = 12,
                    Margin = new Thickness(5, 0)
                }
            };
        }
    }

    public void Draw(EditorInfo info, DownloadStatus? downloadingStatus, int pos, Func<Task> redrawRequest)
    {
        if (pos % 2 == 0)
            cont.Classes.Remove("Odd");
        else
            cont.Classes.Add("Odd");

        this.info = info;
        this.redrawRequest = redrawRequest;
        this.DataContext = downloadingStatus;

        ProductName = info.friendlyVersionName;
        VersionNumber = info.versionName;

        tagLines.Draw(info.CreateTags(), (lbl, _, dat) => lbl.Init(dat));
        Popup_GenericList popupOptions;

        if (info is not EditorInstallInfo installedInfo)
        {
            if (downloadingStatus != null)
            {
                InstallLocation = string.Empty;
                cont_DownloadStatus.IsVisible = true;

                popupOptions = new Popup_GenericList();
                popupOptions.Draw([LanguageHelper.GetLanguageResource("Literal_Cancel")!], OnExtraOptionCallback_Downloading);
                btn_Extra.RegisterPopup(popupOptions);
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            return;
        }

        popupOptions = new Popup_GenericList();
        popupOptions.Draw([LanguageHelper.GetLanguageResource("Literal_Edit")!, "Browse", LanguageHelper.GetLanguageResource("Literal_Delete")!], OnExtraOptionCallback_Installed);
        btn_Extra.RegisterPopup(popupOptions);

        cont_DownloadStatus.IsVisible = false;

        InstallLocation = installedInfo.installLocation;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }

    private async Task OnExtraOptionCallback_Downloading(int _, string __)
    {
        DependencyManager.GetService<IEditorLogic>()!.StopActiveInstall(VersionNumber);
        redrawRequest?.Invoke();
    }

    private async Task OnExtraOptionCallback_Installed(int value, string _)
    {
        switch (value)
        {
            case 0: // Edit
                if (info == null)
                    return;

                await MainWindow.ShowModalAndWait<EditorManagerModal>(async m =>
                {
                    await m.Open(info);
                });
                break;

            case 1: // Browse
                DependencyManager.GetService<IEditorLogic>()!.BrowseToEditor(info);
                break;

            case 2: // Delete
                IEditorLogic logic = DependencyManager.GetService<IEditorLogic>()!;
                string? dir = Directory.GetParent((await logic.GetEditorInstall(VersionNumber))!)!.Parent!.FullName; // rather it fail then give back an invalid result

                if (await DependencyManager.ui!.ShowConfirmationBox(LanguageHelper.GetLanguageResource("Literal_Delete")!, $"Are you sure you want to delete\n{dir}?", LanguageHelper.Button_Cancel, LanguageHelper.Button_Delete) != 1)
                    return;

                await DependencyManager.ui!.LoadProgressive("Deleteing", new LoadRequest("Deleting", async (_, __) => await logic.Delete(VersionNumber)));
                break;
        }
    }
}