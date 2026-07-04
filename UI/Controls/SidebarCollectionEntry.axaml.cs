using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Models.Data;
using UI.Helpers;

namespace UI.Controls;

public partial class SidebarCollectionEntry : UserControl, ISidebarControl
{
    public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<SidebarCollectionEntry, object?>(nameof(Icon));
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<SidebarCollectionEntry, string>(nameof(Label));
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // can never select this collection on its own, therefore this is only ever to inform that its / its children have been deselected
    public bool setSelected { set => UpdateSelection(-1); }

    private ReusableList<SidebarEntry> entries;

    public SidebarCollectionEntry()
    {
        InitializeComponent();
        entries = new ReusableList<SidebarEntry>(container);
    }

    public async Task Init(Func<int, Task> onSelect, Func<Task<CollectionData[]>> dataFetch)
    {
        await entries.DrawAsync(dataFetch, (ui, pos, dat) =>
        {
            const int radius = 10;

            Viewbox iconContainer = new Viewbox();
            Path iconPath = new Path
            {
                Data = new EllipseGeometry(new Rect(0, 0, radius, radius)),
                Fill = new SolidColorBrush(Color.Parse(dat.colour ?? "#fff"))
            };

            iconContainer.Child = iconPath;

            ui.Init(async () => await OnSelect(pos, dat.collectionId));
            ui.Label = dat.collectionName;
            ui.Icon = iconContainer;
        });

        async Task OnSelect(int pos, int dataId)
        {
            await onSelect(dataId);
            UpdateSelection(pos);
        }
    }

    void UpdateSelection(int pos)
    {
        for (int i = 0; i < entries.getElementCount; i++)
        {
            entries[i].setSelected = i == pos;
        }
    }
}