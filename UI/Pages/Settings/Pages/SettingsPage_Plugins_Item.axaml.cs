using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;
using Models.Interfaces;
using UI.Helpers;

namespace UnityHub.UI.Pages.Settings.Pages;

public partial class SettingsPage_Plugins_Item : UserControl
{
    private PluginManifest activeManifest;

    public SettingsPage_Plugins_Item()
    {
        InitializeComponent();

        btn_Settings.RegisterClick(HandleAction);
    }

    public void Draw(PluginManifest manifest, int index)
    {
        if (index % 2 == 0)
            cont.Classes.Remove("Odd");
        else
            cont.Classes.Add("Odd");

        this.DataContext = manifest;
        this.activeManifest = manifest;

        IPlugin? plugin = PluginHelper.GetPlugin(activeManifest);

        if (plugin == null)
        {
            btn_Settings.IsVisible = false;
            lbl_FailedToLoad.IsVisible = true;
        }
        else
        {
            btn_Settings.IsVisible = true;
            lbl_FailedToLoad.IsVisible = false;

            if (plugin.RequiresAuthentication())
            {
                btn_Settings.Label = "Authenticate";
            }
            else
            {
                btn_Settings.Label = "Settings";
            }
        }
    }

    private Task HandleAction() => PluginHelper.GetPlugin(activeManifest)!.RequestSettingsOrAuth();
}