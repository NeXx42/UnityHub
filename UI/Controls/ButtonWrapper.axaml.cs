using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.Helpers;

namespace UI.Controls;

public partial class ButtonWrapper : UserControl
{
    public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<SidebarEntry, object?>(nameof(Icon));
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<ButtonWrapper, string>(nameof(Label));
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public new static readonly StyledProperty<CornerRadius> CornerRadiusProperty = AvaloniaProperty.Register<ButtonWrapper, CornerRadius>(nameof(CornerRadius), defaultValue: new CornerRadius(8));
    public new CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly StyledProperty<object?> PopupContentProperty = AvaloniaProperty.Register<ButtonWrapper, object?>(nameof(PopupContent));
    public object? PopupContent
    {
        get => GetValue(PopupContentProperty);
        set => SetValue(PopupContentProperty, value);
    }

    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<ButtonWrapper, bool>(nameof(IsOpen), defaultValue: false);
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private Action? onClick;

    public ButtonWrapper()
    {
        InitializeComponent();
        btn.PointerPressed += (_, __) => onClick?.Invoke();
    }

    public void RegisterClick(Action callback)
    {
        PopupContent = null;
        onClick = callback;
    }

    public void RegisterPopup(UserControl popup)
    {
        PopupContent = popup;
        onClick = () => IsOpen = true;
    }

    public void RegisterClick(Func<Task> callback)
    {
        onClick = callback.Wrap;
    }
}