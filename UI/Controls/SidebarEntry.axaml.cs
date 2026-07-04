using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using UI.Helpers;

namespace UI.Controls;

public partial class SidebarEntry : UserControl, ISidebarControl
{
    public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<SidebarEntry, object?>(nameof(Icon));
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<SidebarEntry, string>(nameof(Label), defaultValue: "Action");
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    private Action? onSelectCallback;

    public bool setSelected
    {
        set
        {
            if (value)
                cont_Border.Classes.Add("Selected");
            else
                cont_Border.Classes.Remove("Selected");
        }
    }

    public SidebarEntry()
    {
        InitializeComponent();
        cont_Border.PointerPressed += (_, __) => onSelectCallback?.Invoke();
    }

    public void Init(Func<Task> onSelect)
    {
        onSelectCallback = onSelect.Wrap;
    }
}