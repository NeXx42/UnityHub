using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UI.Pages.Settings.Pages;

public partial class SettingsPage_General : UserControl, ISettingsPage
{
    public SettingsPage_General()
    {
        InitializeComponent();
    }

    public UserControl getControl => this;

    public Task OnOpen()
    {
        return Task.CompletedTask;
    }
}