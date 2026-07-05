using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using UI.Controls;
using UI.Helpers;
using UI.Interfaces;
using UI.Pages.Settings.Pages;

namespace UI.Pages.Settings;

public interface ISettingsSidebar : IFrontendPlugin
{
    public void RegisterButtons(ref List<(SidebarEntry sidebarEntry, ISettingsPage ui)> tabs);
}

public partial class Sidebar : UserControl
{
    public static FrontendPluginHandler<ISettingsSidebar> pluginHandler = new();

    private Page? settingsPage;
    private List<(SidebarEntry sidebarEntry, ISettingsPage ui)> tabs = new();

    public Sidebar()
    {
        InitializeComponent();
    }

    public async Task Init(Page settingsPage)
    {
        this.settingsPage = settingsPage;

        tabs = [
            (new SidebarEntry(){
                Label = "Editor",
                Icon = new Viewbox(){
                    Child = new Path(){
                        Data = Geometry.Parse("M7.752.066a.5.5 0 0 1 .496 0l3.75 2.143a.5.5 0 0 1 .252.434v3.995l3.498 2A.5.5 0 0 1 16 9.07v4.286a.5.5 0 0 1-.252.434l-3.75 2.143a.5.5 0 0 1-.496 0l-3.502-2-3.502 2.001a.5.5 0 0 1-.496 0l-3.75-2.143A.5.5 0 0 1 0 13.357V9.071a.5.5 0 0 1 .252-.434L3.75 6.638V2.643a.5.5 0 0 1 .252-.434zM4.25 7.504 1.508 9.071l2.742 1.567 2.742-1.567zM7.5 9.933l-2.75 1.571v3.134l2.75-1.571zm1 3.134 2.75 1.571v-3.134L8.5 9.933zm.508-3.996 2.742 1.567 2.742-1.567-2.742-1.567zm2.242-2.433V3.504L8.5 5.076V8.21zM7.5 8.21V5.076L4.75 3.504v3.134zM5.258 2.643 8 4.21l2.742-1.567L8 1.076zM15 9.933l-2.75 1.571v3.134L15 13.067zM3.75 14.638v-3.134L1 9.933v3.134z")
                    }
                }
            }, new SettingsPage_Editors()),
            (new SidebarEntry(){
                Label = "General",
                Icon = new Viewbox(){
                    Child = new Path(){
                        Data = Geometry.Parse("M7.752.066a.5.5 0 0 1 .496 0l3.75 2.143a.5.5 0 0 1 .252.434v3.995l3.498 2A.5.5 0 0 1 16 9.07v4.286a.5.5 0 0 1-.252.434l-3.75 2.143a.5.5 0 0 1-.496 0l-3.502-2-3.502 2.001a.5.5 0 0 1-.496 0l-3.75-2.143A.5.5 0 0 1 0 13.357V9.071a.5.5 0 0 1 .252-.434L3.75 6.638V2.643a.5.5 0 0 1 .252-.434zM4.25 7.504 1.508 9.071l2.742 1.567 2.742-1.567zM7.5 9.933l-2.75 1.571v3.134l2.75-1.571zm1 3.134 2.75 1.571v-3.134L8.5 9.933zm.508-3.996 2.742 1.567 2.742-1.567-2.742-1.567zm2.242-2.433V3.504L8.5 5.076V8.21zM7.5 8.21V5.076L4.75 3.504v3.134zM5.258 2.643 8 4.21l2.742-1.567L8 1.076zM15 9.933l-2.75 1.571v3.134L15 13.067zM3.75 14.638v-3.134L1 9.933v3.134z")
                    }
                }
            }, new SettingsPage_General())
        ];

        pluginHandler.Execute(p => p.RegisterButtons(ref tabs));

        for (int i = 0; i < tabs.Count; i++)
        {
            int temp = i;
            (SidebarEntry entry, ISettingsPage ui) = tabs[temp];

            cont_Entries.Children.Add(entry);
            entry.Init(() => UpdateSelection(temp));

            settingsPage.AddPage(ui);
        }

        await UpdateSelection(0);
    }

    private async Task UpdateSelection(int id)
    {
        await settingsPage!.OpenPage(id);

        for (int i = 0; i < tabs.Count; i++)
        {
            (SidebarEntry entry, _) = tabs[i];

            if (i == id)
                entry.setSelected = true;
            else
                entry.setSelected = false;
        }
    }
}