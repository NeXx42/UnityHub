using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UI.Pages.Settings;

public interface ISettingsPage
{
    public UserControl getControl { get; }
    public Task OnOpen();
}

public partial class Page : UserControl, IPage
{
    private List<ISettingsPage> pages = new List<ISettingsPage>();

    public Page()
    {
        InitializeComponent();
    }


    public async Task<Control> Show()
    {
        IsVisible = true;
        Sidebar sidebar = new Sidebar();
        await sidebar.Init(this);

        return sidebar;
    }

    public void AddPage(ISettingsPage page)
    {
        cont.Children.Add(page.getControl);
        pages.Add(page);
    }

    public async Task OpenPage(int pos)
    {
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].getControl.IsVisible = pos == i;

            if (pos == i)
                await pages[i].OnOpen();
        }
    }

    public Task Close()
    {
        IsVisible = false;
        return Task.CompletedTask;
    }
}