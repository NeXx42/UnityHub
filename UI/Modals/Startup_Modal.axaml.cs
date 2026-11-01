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
    private TaskCompletionSource? task;
    private bool allowDismissal;

    public Startup_Modal()
    {
        InitializeComponent();
    }

    public ModalContainer setContainer { set => _ = value; }
    public bool canDismiss => allowDismissal;

    public Task Launch(bool isFirstLaunch)
    {
        task = new TaskCompletionSource();

        if (isFirstLaunch)
        {
            allowDismissal = false;
            Startup_FirstSetup menu = new Startup_FirstSetup();

            cont_Item.Children.Add(menu);

            return menu.Handle();
        }
        else
        {
            allowDismissal = true;
        }

        return task.Task;
    }
}