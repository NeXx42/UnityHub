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
                new SolidColorBrush(Color.Parse("#4C78A8")),
                new SolidColorBrush(Color.Parse("#477FA3")),
                new SolidColorBrush(Color.Parse("#43859D")),
                new SolidColorBrush(Color.Parse("#3F8B96")),
                new SolidColorBrush(Color.Parse("#45918D")),
                new SolidColorBrush(Color.Parse("#519780")),
                new SolidColorBrush(Color.Parse("#639B73")),
                new SolidColorBrush(Color.Parse("#789E68")),
                new SolidColorBrush(Color.Parse("#8D9E60")),
                new SolidColorBrush(Color.Parse("#A19B5C"))
            };
        }

        InitializeComponent();
        sliceList = new ReusableList<Border>(cont_Slices, () =>
        {
            Border b = new Border();
            b.Margin = new Thickness(0, 0, 1, 0);

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

        slices.Add(("Rest", groupedSpace));
        slices.Add(("Filesystem", unknownSpace));

        sliceList.Draw(slices, (ui, pos, dat) =>
        {
            ui.Width = dat.Item2 * pixelsPerByte;
            ui.Background = sliceColours![pos % sliceColours.Length];
        });
    }
}