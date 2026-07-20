using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Helpers;
using UI.Popups;

namespace UI.Pages.Settings.Pages;

public partial class SettingsPage_Editors_InstalledVersion : UserControl, INotifyPropertyChanged
{
    public string ProductName { get; set; } = "";
    public string InstallLocation { get; set; } = "";

    private ReusableList<Border> tagLines;
    private ReusableList<Border> platforms;

    private Func<Task>? redrawRequest;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public SettingsPage_Editors_InstalledVersion()
    {
        InitializeComponent();

        tagLines = new ReusableList<Border>(cont_Tags, CreateTag);
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

    public void Draw(EditorInfo info, DownloadStatus? downloadingStatus, Func<Task> redrawRequest)
    {
        this.redrawRequest = redrawRequest;
        this.DataContext = downloadingStatus;

        ProductName = info.versionName;

        List<(string, string)> tags = new();

        if (!string.IsNullOrEmpty(info.stream)) tags.Add(CreateTag(info.stream, info.stream));
        if (!string.IsNullOrEmpty(info.label?.labelText)) tags.Add(CreateTag(info.label.Value.labelText, info.label.Value.colour ?? "WARNING"));

        tagLines.Draw(tags, (lbl, _, dat) =>
        {
            Color c = Color.Parse(dat.Item2);

            (lbl.Child as Label)!.Content = dat.Item1;
            (lbl.Child as Label)!.Foreground = new SolidColorBrush(c);

            lbl.Background = new SolidColorBrush(new Color(100, c.R, c.G, c.B));
        });

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
        popupOptions.Draw(["Manage", "Delete"], OnExtraOptionCallback);
        btn_Extra.RegisterPopup(popupOptions);

        cont_DownloadStatus.IsVisible = false;

        InstallLocation = installedInfo.installLocation;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));

        (string, string) CreateTag(string msg, string colourName)
        {
            string? hexColour = null;

            switch (colourName)
            {
                case "Beta":
                case "Alpha":
                    hexColour = "#006CDF";
                    break;

                case "WARNING":
                    hexColour = "#FFC53D";
                    break;

                case "ERROR":
                    hexColour = "#E5484D";
                    break;
            }

            return (msg, hexColour ?? "#ffffff");
        }
    }

    private async Task OnExtraOptionCallback(int _, string value)
    {
        switch (value)
        {
            case "Cancel":

                DependencyManager.GetService<IEditorLogic>()!.StopActiveInstall(ProductName);
                redrawRequest?.Invoke();
                break;
        }
    }
}