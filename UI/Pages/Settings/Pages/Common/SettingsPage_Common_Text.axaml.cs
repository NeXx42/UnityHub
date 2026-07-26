using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models;
using Models.Enums;
using Models.Interfaces;

namespace UI.Pages.Settings.Pages.Common;

public partial class SettingsPage_Common_Text : UserControl, ISettingsPageSetting
{
    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<SettingsPage_Common_Text, string>(nameof(Label), "");
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    private string? curVal;
    private ConfigEntry? key;
    private bool isSupported = true;

    public SettingsPage_Common_Text()
    {
        InitializeComponent();

        inp_Val.TextChanged += (_, __) =>
        {
            ToggleButtonAvability(!(curVal ?? "").Equals(inp_Val.Text));
        };

        btn_Save.RegisterClick(Save);
    }

    public ISettingsPageSetting Init(ConfigEntry key, bool supportWindows = true, bool supportLinux = true)
    {
        if (GlobalConfig.isOnLinux && !supportLinux)
        {
            IsVisible = false;
            isSupported = false;

            return this;
        }

        if (!GlobalConfig.isOnLinux && !supportWindows)
        {
            IsVisible = false;
            isSupported = false;

            return this;
        }

        this.key = key;
        return this;
    }

    public async Task Load(IConfigLogic configProvider)
    {
        if (!isSupported)
            return;

        ToggleButtonAvability(false);

        if (!key.HasValue)
            return;

        curVal = await configProvider.Get(key.Value, "");
        inp_Val.Text = curVal;
    }

    private async Task Save()
    {
        if (!key.HasValue)
            return;

        curVal = inp_Val.Text;
        ToggleButtonAvability(false);

        await DependencyManager.GetService<IConfigLogic>()!.Set(key.Value, curVal, true);
    }

    private void ToggleButtonAvability(bool to)
    {
        btn_Save.IsEnabled = to;

        if (to)
            btn_Save.Classes.Add("Primary");
        else
            btn_Save.Classes.Remove("Primary");
    }
}