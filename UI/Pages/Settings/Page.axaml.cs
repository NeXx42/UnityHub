using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UI.Pages.Settings;

public partial class Page : UserControl, IPage
{
    public Page()
    {
        InitializeComponent();
    }


    public Task<Control> Show()
    {
        IsVisible = true;
        Sidebar sidebar = new Sidebar();

        return Task.FromResult<Control>(sidebar);
    }

    public Task Close()
    {
        IsVisible = false;
        return Task.CompletedTask;
    }
}