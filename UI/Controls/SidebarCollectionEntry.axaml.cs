using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;

namespace UI.Controls;

public partial class SidebarCollectionEntry : UserControl
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

    public SidebarCollectionEntry()
    {
        InitializeComponent();
        DataContext = this;
    }

    public void Init(Func<Task> onSelect, Func<Task<CollectionData[]>> dataFetch)
    {
        container.Children.Add(CreateEntry());

        UserControl CreateEntry()
        {
            SidebarEntry sidebar = new SidebarEntry();
            sidebar.Label = "Test";

            return sidebar;
        }
    }
}