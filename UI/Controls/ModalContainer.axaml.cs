using System;
using System.Threading;
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
    private CancellationTokenSource cancelToken;

    private UserControl? activeControl;

    public ModalContainer()
    {
        InitializeComponent();

        cancelToken = new CancellationTokenSource();
        Container.PointerPressed += (_, __) => requestCloserEvent?.Invoke();
    }

    public T ShowModal<T>(out CancellationToken token) where T : UserControl, IModal
    {
        token = cancelToken.Token;

        T control = Activator.CreateInstance<T>();
        control.container = this;

        requestCloserEvent = () => cancelToken.Cancel();

        activeModal = control;
        activeControl = control;

        activeControl.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        activeControl.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        activeControl.PointerPressed += (_, e) => e.Handled = true;

        Container.Children.Add(activeControl);
        return control;
    }
}