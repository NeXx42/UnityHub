using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UI.Pages.Settings.Pages;

public partial class SettingsPage_Themes : UserControl, ISettingsPage
{
    public SettingsPage_Themes()
    {
        InitializeComponent();
    }

    public UserControl getControl => this;

    public Task OnOpen()
    {
        return Task.CompletedTask;
    }
}