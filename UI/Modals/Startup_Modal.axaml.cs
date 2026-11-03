using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.Controls;
using UI.Modals;
using UnityHub.UI.Modals.Startup;

namespace UnityHub.UI.Modals;

public partial class Startup_Modal : UserControl, IModal
{
    private bool allowDismissal;

    public Startup_Modal()
    {
        InitializeComponent();
    }

    public ModalContainer setContainer { set => _ = value; }
    public bool canDismiss => allowDismissal;

    public Task FirstLaunchSetup()
    {
        allowDismissal = false;
        Startup_FirstSetup menu = new Startup_FirstSetup();

        cont_Item.Child = menu;
        return menu.Handle();
    }

    public Task UpdateLaunch(string msg)
    {
        allowDismissal = true;
        Startup_Update menu = new Startup_Update();

        cont_Item.Child = menu;
        return menu.Handle(msg);
    }
}