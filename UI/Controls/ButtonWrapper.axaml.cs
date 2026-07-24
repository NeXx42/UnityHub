using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Logic;
using UI.Helpers;
using UI.Interfaces;

namespace UI.Controls;

public partial class ButtonWrapper : UserControl
{
    public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<ButtonWrapper, object?>(nameof(Icon));
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    private bool _hasIcon;
    public static readonly DirectProperty<ButtonWrapper, bool> HasIconProperty = AvaloniaProperty.RegisterDirect<ButtonWrapper, bool>(nameof(HasIcon), o => o._hasIcon);

    public bool HasIcon
    {
        get => _hasIcon;
        private set => SetAndRaise(HasIconProperty, ref _hasIcon, value);
    }


    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<ButtonWrapper, string>(nameof(Label), "");
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }
    //defaultValue: ThemeHelper.GetValueFromTheme<CornerRadius>("CornerRadius_Element")
    public new static readonly StyledProperty<CornerRadius> CornerRadiusProperty = AvaloniaProperty.Register<ButtonWrapper, CornerRadius>(nameof(CornerRadius));
    public new CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly StyledProperty<object?> PopupContentProperty = AvaloniaProperty.Register<ButtonWrapper, object?>(nameof(PopupContent));
    public object? PopupContent
    {
        get => GetValue(PopupContentProperty);
        set => SetValue(PopupContentProperty, value);
    }
    private IPopup? PopupData;

    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<ButtonWrapper, bool>(nameof(IsOpen), defaultValue: false);
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static new readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty = AvaloniaProperty.Register<ButtonWrapper, HorizontalAlignment>(nameof(HorizontalContentAlignment), HorizontalAlignment.Left);
    public new HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    public static readonly StyledProperty<bool> HasPopupIconProperty = AvaloniaProperty.Register<ButtonWrapper, bool>(nameof(HasPopupIcon), false);
    public bool HasPopupIcon
    {
        get => GetValue(HasPopupIconProperty);
        set => SetValue(HasPopupIconProperty, value);
    }

    public static readonly StyledProperty<PlacementMode> PopupPlacementTargetProperty = AvaloniaProperty.Register<ButtonWrapper, PlacementMode>(nameof(PopupPlacementTarget), PlacementMode.BottomEdgeAlignedLeft);
    public PlacementMode PopupPlacementTarget
    {
        get => GetValue(PopupPlacementTargetProperty);
        set => SetValue(PopupPlacementTargetProperty, value);
    }

    private Action? onClick;

    public ButtonWrapper()
    {
        InitializeComponent();
        btn.PointerPressed += (_, __) => onClick?.Invoke();

        IconProperty.Changed.AddClassHandler<ButtonWrapper>((x, e) => x.HasIcon = e.NewValue != null);
    }

    public void RegisterClick(Action callback)
    {
        PopupContent = null;
        onClick = () => ClickWrapper(callback);
    }
    public void RegisterClick(Func<Task> callback) => onClick = () => ClickWrapper(callback.Wrap);

    public void RegisterPopup<T>() where T : UserControl, IPopup
        => RegisterPopup(Activator.CreateInstance<T>());

    public void RegisterPopup<T>(T popup) where T : UserControl, IPopup
    {
        PopupContent = popup;
        PopupData = popup;

        onClick = () => HandlePopup().Wrap();

        async Task HandlePopup()
        {
            MainWindow.ClearFocus();

            IsOpen = true;
            await popup.Show();
            IsOpen = false;
        }
    }


    private void ClickWrapper(Action callback)
    {
        MainWindow.ClearFocus();

        try
        {
            callback?.Invoke();
        }
        catch (Exception e)
        {
            DependencyManager.ui!.ShowMessageBox(e).Wrap();
        }
    }
}