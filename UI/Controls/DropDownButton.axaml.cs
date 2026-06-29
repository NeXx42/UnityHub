using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.Controls;
using UI.Helpers;

namespace UI.Controls;

public partial class DropDownButton : UserControl
{
    public static readonly StyledProperty<string> ButtonLabelProperty = AvaloniaProperty.Register<DropDownButton, string>(nameof(ButtonLabel), defaultValue: "Action");
    public string ButtonLabel
    {
        get => GetValue(ButtonLabelProperty);
        set => SetValue(ButtonLabelProperty, value);
    }

    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<DropDownButton, bool>(nameof(IsOpen), defaultValue: false);
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private Action? onClick;
    private ReusableList<ButtonWrapper> buttonList;

    public DropDownButton()
    {
        InitializeComponent();
        DataContext = this;

        buttonList = new ReusableList<ButtonWrapper>(container);

        btn.Click += (_, __) => onClick?.Invoke();
    }


    public void RegisterOptions(IEnumerable<string> options, Func<int, Task> callback)
    {
        buttonList.Draw(options, DrawBtn);

        void DrawBtn(ButtonWrapper btn, int pos, string data)
        {
            btn.Label = data;
            btn.RegisterClick(() => AsyncHelper.WrapTask(pos, callback));
        }
    }

    public void RegisterClick(Func<Task> callback)
    {
        onClick = () => AsyncHelper.WrapTask(callback);
    }
}