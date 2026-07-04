using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.Controls;

namespace UI.Modals;

public partial class CreateProjectModal : UserControl, IModal
{
    public CreateProjectModal()
    {
        InitializeComponent();
    }

    public bool killable => true;
    public ModalContainer setContainer { set => _ = value; }

    public Task Kill()
    {
        throw new System.NotImplementedException();
    }
}