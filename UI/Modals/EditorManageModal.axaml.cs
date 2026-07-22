using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;
using UI.Controls;

namespace UI.Modals;

public partial class EditorManageModal : UserControl, IModal
{
    public EditorManageModal()
    {
        InitializeComponent();
    }

    public ModalContainer setContainer { set => _ = value; }
    public bool canDismiss => true;

    public Task Open(EditorInfo info)
    {
        TaskCompletionSource task = new TaskCompletionSource();
        this.DataContext = info;

        return task.Task;
    }
}