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

        ProductName = info.versionName;

        tagLines.Draw(info.CreateTags(), (lbl, _, dat) => lbl.Init(dat));
        Popup_GenericList popupOptions;

        if (info is not EditorInstallInfo installedInfo)
        {
            if (downloadingStatus != null)
            {
                InstallLocation = string.Empty;
                cont_DownloadStatus.IsVisible = true;

                popupOptions = new Popup_GenericList();
                popupOptions.Draw(["Cancel"], OnExtraOptionCallback);
                btn_Extra.RegisterPopup(popupOptions);
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            return;
        }

        popupOptions = new Popup_GenericList();
        popupOptions.Draw(["Manage", "Browse", "Delete"], OnExtraOptionCallback);
        btn_Extra.RegisterPopup(popupOptions);

        cont_DownloadStatus.IsVisible = false;

        InstallLocation = installedInfo.installLocation;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }

    private async Task OnExtraOptionCallback(int _, string value)
    {
        switch (value)
        {
            case "Cancel":
                DependencyManager.GetService<IEditorLogic>()!.StopActiveInstall(ProductName);
                redrawRequest?.Invoke();
                break;

            case "Delete":
                IEditorLogic logic = DependencyManager.GetService<IEditorLogic>()!;
                string? dir = Directory.GetParent((await logic.GetEditorInstall(ProductName))!)!.Parent!.FullName; // rather it fail then give back an invalid result

                if (await DependencyManager.ui!.ShowConfirmationBox("Delete", $"Are you sure you want to delete\n{dir}?", new ConfirmationButton("Cancel"), new ConfirmationButton("Delete", true)) != 1)
                    return;

                await logic.Delete(ProductName);
                break;

            case "Browse":
                DependencyManager.GetService<IEditorLogic>()!.BrowseToEditor(info);
                break;

            case "Manage":
                if (info == null)
                    return;

                await MainWindow.ShowModalAndWait<EditorManagerModal>(async m =>
                {
                    await m.Open(info);
                });
                break;
        }
    }
}