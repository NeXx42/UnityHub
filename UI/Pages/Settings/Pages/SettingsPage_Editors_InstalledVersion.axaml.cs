using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Models.Data;
using UI.Helpers;

namespace UI.Pages.Settings.Pages;

public partial class SettingsPage_Editors_InstalledVersion : UserControl, INotifyPropertyChanged
{
    public string ProductName { get; set; } = "";
    public string InstallLocation { get; set; } = "";

    private ReusableList<Border> tagLines;
    private ReusableList<Border> platforms;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public SettingsPage_Editors_InstalledVersion()
    {
        InitializeComponent();

        tagLines = new ReusableList<Border>(cont_Tags, () =>
        {
            Border border = new Border()
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
            };
            Label l = new Label();
            l.FontSize = 12;

            border.Child = l;
            return border;
        });
        platforms = new ReusableList<Border>(cont_Platforms, () =>
        {
            Border border = new Border()
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
            };
            Label l = new Label();
            l.FontSize = 12;

            border.Child = l;
            return border;
        });
    }

    public void Draw(EditorInfo info)
    {
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

        if (info is not EditorInstallInfo installedInfo)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            return;
        }

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
}