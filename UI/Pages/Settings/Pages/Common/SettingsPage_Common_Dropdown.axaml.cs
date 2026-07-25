using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Enums;
using Models.Interfaces;

namespace UI.Pages.Settings.Pages.Common;

public partial class SettingsPage_Common_Dropdown : UserControl, INotifyPropertyChanged, ISettingsPageSetting
{
    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<SettingsPage_Common_Text, string>(nameof(Label), "");
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    private ConfigEntry? key;
    private bool isActive = false;

    public SettingsPage_Common_Dropdown()
    {
        InitializeComponent();

        inp_Options.SelectionChanged += (_, __) => _ = Save();
    }


    public ISettingsPageSetting Init(ConfigEntry key)
    {
        this.key = key;
        return this;
    }

    public SettingsPage_Common_Dropdown RegisterType<T>() where T : Enum
    {
        string[] values = Enum.GetNames(typeof(T));
        return RegisterType(values);
    }


    public SettingsPage_Common_Dropdown RegisterType(string[] options)
    {
        isActive = false;

        inp_Options.ItemsSource = options;
        inp_Options.SelectedIndex = 0;

        isActive = true;
        return this;
    }

    public async Task Load(IConfigLogic configProvider)
    {
        isActive = false;

        string res = await configProvider.Get(key!.Value, "");
        inp_Options.SelectedValue = res;

        if (inp_Options.SelectedIndex == -1)
            inp_Options.SelectedIndex = 0;

        isActive = true;
    }

    private async Task Save()
    {
        if (!isActive)
            return;

        await DependencyManager.GetService<IConfigLogic>()!.Set(key!.Value, inp_Options.SelectedValue, true);
    }
}