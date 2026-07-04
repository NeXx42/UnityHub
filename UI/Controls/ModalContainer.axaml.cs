using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.Helpers;
using UI.Modals;

namespace UI.Controls;

public partial class ModalContainer : UserControl
{
    public Action? requestCloserEvent { private set; get; }

    private IModal? activeModal;
    private UserControl? activeControl;

    public ModalContainer()
    {
        InitializeComponent();

        Container.PointerPressed += (_, __) => requestCloserEvent?.Invoke();
    }

    public T ShowModal<T>(int pos, Func<int, Task> requestClosure) where T : UserControl, IModal
    {
        requestCloserEvent = () => requestClosure(pos).Wrap();

        T control = Activator.CreateInstance<T>();
        control.setContainer = this;

        activeModal = control;
        activeControl = control;

        activeControl.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        activeControl.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        activeControl.PointerPressed += (_, e) => e.Handled = true;

        Container.Children.Add(activeControl);
        return control;
    }
}