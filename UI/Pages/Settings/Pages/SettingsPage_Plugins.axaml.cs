using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.Helpers;
using UI.Pages.Settings;

namespace UnityHub.UI.Pages.Settings.Pages;

public partial class SettingsPage_Plugins : UserControl, ISettingsPage
{
    private ReusableList<SettingsPage_Plugins_Item> items;

    public SettingsPage_Plugins()
    {
        InitializeComponent();

        items = new ReusableList<SettingsPage_Plugins_Item>(cont_ActivePlugins);
    }

    public UserControl getControl => this;

    public Task OnOpen()
    {
        items.Draw(PluginHelper.GetActivePlugins(), (ui, i, dat) => ui.Draw(dat, i));

        return Task.CompletedTask;
    }
}