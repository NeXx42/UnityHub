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
                    Height = 17,
                    Child = new Path(){
                        Data = Geometry.Parse("m 12.8497 1 v 3.9324 l 3.4868 2.0108 c 0.0279 0.0167 0.0501 0.0446 0.0668 0.0724 c 0.0223 0.0278 0.0279 0.0613 0.0279 0.0947 c 0 0.0334 -0.0056 0.0668 -0.0279 0.1003 c -0.0167 0.0278 -0.039 0.0501 -0.0668 0.0724 l -4.1441 2.384 c -0.0557 0.039 -0.1281 0.0557 -0.195 0.0557 c -0.0668 0 -0.1337 -0.0167 -0.195 -0.0557 l -4.1441 -2.384 c -0.0279 -0.0167 -0.0501 -0.0446 -0.0668 -0.0724 c -0.0167 -0.0279 -0.0279 -0.0613 -0.0279 -0.0947 c 0 -0.0334 0.0111 -0.0668 0.0279 -0.1003 c 0.0167 -0.0279 0.039 -0.0501 0.0668 -0.0724 l 3.4868 -2.0052 v -3.938 l -8.8953 5.13 v 10.2544 l 3.4144 -1.9662 v -4.0215 c 0 -0.0334 0.0056 -0.0668 0.0223 -0.0947 c 0.0167 -0.0279 0.0446 -0.0501 0.0724 -0.0724 c 0.0278 -0.0167 0.0613 -0.0223 0.1003 -0.0223 c 0.0334 0 0.0668 0.0056 0.0947 0.0223 l 4.1441 2.3895 c 0.0613 0.0334 0.1114 0.0836 0.1448 0.1448 c 0.0334 0.0557 0.0501 0.1225 0.0501 0.1949 v 4.7735 c 0 0.0334 -0.0056 0.0668 -0.0278 0.1003 c -0.0167 0.0279 -0.039 0.0557 -0.0668 0.0724 c -0.0334 0.0167 -0.0668 0.0223 -0.1003 0.0223 c -0.0334 0 -0.0668 -0.0056 -0.0947 -0.0223 l -3.4868 -2.0108 l -3.4144 1.9662 l 8.8953 5.13 l 8.8953 -5.13 l -3.4144 -1.9662 l -3.4868 2.0108 c -0.0278 0.0167 -0.0613 0.0223 -0.0947 0.0223 c -0.0334 0 -0.0668 -0.0111 -0.0947 -0.0279 c -0.0334 -0.0167 -0.0557 -0.039 -0.0724 -0.0668 c -0.0167 -0.0334 -0.0279 -0.0668 -0.0279 -0.1003 v -4.7735 c 0 -0.0724 0.0167 -0.1393 0.0501 -0.1949 c 0.039 -0.0613 0.0836 -0.1114 0.1448 -0.1448 l 4.1441 -2.3895 c 0.0278 -0.0167 0.0613 -0.0223 0.0947 -0.0223 c 0.0334 0 0.0668 0.0056 0.1003 0.0223 c 0.0278 0.0223 0.0501 0.0446 0.0668 0.0724 c 0.0223 0.0279 0.0279 0.0613 0.0279 0.1003 v 4.016 l 3.4144 1.9662 v -10.2544 l -8.8953 -5.13 z")
                    }
                }
            }, new SettingsPage_Editors()),
            (new SidebarEntry(){
                Label = "General",
                Icon = new Viewbox(){
                    Child = new Path(){
                        Data = Geometry.Parse("M9.405 1.05c-.413-1.4-2.397-1.4-2.81 0l-.1.34a1.464 1.464 0 0 1-2.105.872l-.31-.17c-1.283-.698-2.686.705-1.987 1.987l.169.311c.446.82.023 1.841-.872 2.105l-.34.1c-1.4.413-1.4 2.397 0 2.81l.34.1a1.464 1.464 0 0 1 .872 2.105l-.17.31c-.698 1.283.705 2.686 1.987 1.987l.311-.169a1.464 1.464 0 0 1 2.105.872l.1.34c.413 1.4 2.397 1.4 2.81 0l.1-.34a1.464 1.464 0 0 1 2.105-.872l.31.17c1.283.698 2.686-.705 1.987-1.987l-.169-.311a1.464 1.464 0 0 1 .872-2.105l.34-.1c1.4-.413 1.4-2.397 0-2.81l-.34-.1a1.464 1.464 0 0 1-.872-2.105l.17-.31c.698-1.283-.705-2.686-1.987-1.987l-.311.169a1.464 1.464 0 0 1-2.105-.872zM8 10.93a2.929 2.929 0 1 1 0-5.86 2.929 2.929 0 0 1 0 5.858z")
                    }
                }
            }, new SettingsPage_General()),
            (new SidebarEntry(){
                Label = "Collections",
                Icon = new Viewbox(){
                    Child = new Path(){
                        Data = Geometry.Parse("M2.5 3.5a.5.5 0 0 1 0-1h11a.5.5 0 0 1 0 1zm2-2a.5.5 0 0 1 0-1h7a.5.5 0 0 1 0 1zM0 13a1.5 1.5 0 0 0 1.5 1.5h13A1.5 1.5 0 0 0 16 13V6a1.5 1.5 0 0 0-1.5-1.5h-13A1.5 1.5 0 0 0 0 6zm1.5.5A.5.5 0 0 1 1 13V6a.5.5 0 0 1 .5-.5h13a.5.5 0 0 1 .5.5v7a.5.5 0 0 1-.5.5z")
                    }
                }
            }, new SettingsPage_Collections()),
            (new SidebarEntry(){
                Label = "Themes",
                Icon = new Viewbox(){
                    Child = new Path(){
                        Data = Geometry.Parse("M12.433 10.07C14.133 10.585 16 11.15 16 8a8 8 0 1 0-8 8c1.996 0 1.826-1.504 1.649-3.08-.124-1.101-.252-2.237.351-2.92.465-.527 1.42-.237 2.433.07M8 5a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3m4.5 3a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3M5 6.5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0m.5 6.5a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3")
                    }
                }
            }, new SettingsPage_Themes())
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