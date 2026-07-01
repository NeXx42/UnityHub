using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UI.Controls;

public partial class SidebarEntry : UserControl
{

    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<ButtonWrapper, string>(nameof(Label), defaultValue: "Action");
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    private Action? onSelectCallback;


    public SidebarEntry()
    {
        InitializeComponent();
        DataContext = this;

        cont_Border.PointerPressed += (_, __) => onSelectCallback?.Invoke();
    }

    public void Init(Func<Task> onSelect)
    {
        onSelectCallback = () => _ = onSelect();
    }
}