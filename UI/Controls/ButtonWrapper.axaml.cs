using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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

    private Action? onClick;

    public ButtonWrapper()
    {
        InitializeComponent();
        btn.PointerPressed += (_, __) => onClick?.Invoke();
    }

    public void RegisterClick(Action callback)
    {
        onClick = callback;
    }

    public void RegisterClick(Func<Task> callback)
    {
        onClick = () => _ = HandleCallback();

        async Task HandleCallback()
        {
            try
            {
                await callback();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}