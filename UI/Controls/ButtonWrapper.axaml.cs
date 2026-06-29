using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UI.Controls;

public partial class ButtonWrapper : UserControl
{
    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<ButtonWrapper, string>(nameof(Label), defaultValue: "Action");
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }


    private Action? onClick;

    public ButtonWrapper()
    {
        InitializeComponent();
        DataContext = this;

        btn.Click += (_, __) => onClick?.Invoke();
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