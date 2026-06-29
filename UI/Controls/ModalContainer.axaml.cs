using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UI.Controls;

public partial class ModalContainer : UserControl
{
    private Action? requestCloserEvent;
    private UserControl? activeControl;

    public ModalContainer()
    {
        InitializeComponent();

        Container.PointerPressed += (_, __) => requestCloserEvent?.Invoke();
    }

    public T ShowModal<T>(int pos, Action<int> requestClosure) where T : UserControl
    {
        requestCloserEvent = () => requestClosure(pos);

        T control = Activator.CreateInstance<T>();
        activeControl = control;

        activeControl.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        activeControl.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        activeControl.PointerPressed += (_, e) => e.Handled = true;

        Container.Children.Add(activeControl);
        return control;
    }
}