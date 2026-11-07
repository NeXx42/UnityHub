using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UI.Pages.Settings.Pages.Common;

public partial class SettingsPage_Common_Button : UserControl
{
    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<SettingsPage_Common_Text, string>(nameof(Label), "");
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly StyledProperty<string> ButtonTextProperty = AvaloniaProperty.Register<SettingsPage_Common_Text, string>(nameof(ButtonText), "");
    public string ButtonText
    {
        get => GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public SettingsPage_Common_Button()
    {
        InitializeComponent();
    }

    public void Setup(Func<Task> callback)
    {
        btn.RegisterClick(callback);
    }
}