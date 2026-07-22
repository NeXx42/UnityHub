using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Models.Data;
using UI.Helpers;

namespace UI.Pages.HomePage;

public partial class Sidebar_DriveUsage : UserControl, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged;
    public string DriveName { get; set; } = "";

    private ReusableList<Border> sliceList;
    private static IBrush[]? sliceColours;

    private double? elementWidth = null;

    public Sidebar_DriveUsage()
    {
        if (sliceColours == null)
        {
            sliceColours = new[]
            {
                new SolidColorBrush(Color.Parse("#4E79A7")),
                new SolidColorBrush(Color.Parse("#5B8DB8")),
                new SolidColorBrush(Color.Parse("#4C9FB1")),
                new SolidColorBrush(Color.Parse("#59A89C")),
                new SolidColorBrush(Color.Parse("#72A98A")),
                new SolidColorBrush(Color.Parse("#8FAF78")),
                new SolidColorBrush(Color.Parse("#A6A96B")),
                new SolidColorBrush(Color.Parse("#B39B63")),
                new SolidColorBrush(Color.Parse("#A9826B")),
                new SolidColorBrush(Color.Parse("#96758C"))
            };
        }

        InitializeComponent();
        sliceList = new ReusableList<Border>(cont_Slices, () =>
        {
            Border b = new Border();
            b.Margin = new Thickness(0, 0, 1, 0);

            ToolTip t = new ToolTip();

            b.Child = t;
            return b;
        });
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        elementWidth = e.NewSize.Width;
        base.OnSizeChanged(e);
    }

    public async Task DrawSlices(DriveInfo drive, IEnumerable<ProjectInfo> projs)
    {
        for (int i = 0; i < 100; i++) // just give up after this..
        {
            if (elementWidth.HasValue)
                break;

            await Task.Delay(10);
        }

        DriveName = drive.Name;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DriveName)));

        double pixelsPerByte = (elementWidth ?? 150d) / drive.TotalSize;

        List<(string, long)> slices = new List<(string, long)>();

        long unknownSpace = drive.TotalSize - drive.TotalFreeSpace;
        long groupedSpace = 0;

        foreach (ProjectInfo proj in projs)
        {
            unknownSpace -= proj.size ?? 0;

            if (slices.Count < 5 && proj.size > 1_000)
                slices.Add((proj.name, proj.size.Value));
            else
                groupedSpace += proj.size ?? 0;
        }

        slices.Add(("Other Projects", groupedSpace));
        slices.Add(("Filesystem", unknownSpace));

        sliceList.Draw(slices, (ui, pos, dat) =>
        {
            ui.Width = dat.Item2 * pixelsPerByte;
            ToolTip.SetTip(ui, dat.Item1);

            ui.Background = sliceColours![pos % sliceColours.Length];
        });
    }
}