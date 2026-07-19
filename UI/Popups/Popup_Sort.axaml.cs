using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.Controls;
using UI.Helpers;
using UI.Interfaces;

namespace UI.Popups;

public partial class Popup_Sort : UserControl, IPopup
{
    private int selectedOption = 0;
    private TaskCompletionSource? openTask;

    private Action<int>? onUpdateAction;
    private ReusableList<ButtonWrapper> options;

    public Popup_Sort()
    {
        InitializeComponent();
        options = new ReusableList<ButtonWrapper>(cont, () =>
        {
            ButtonWrapper btn = new ButtonWrapper();
            btn.Classes.Add("Transparent");

            return btn;
        });
    }

    public void Draw(int selectedOption, IEnumerable<string> values, Func<int, Task> onUpdateCallback)
        => Draw(selectedOption, values, (v) => onUpdateCallback.WrapTask(v));

    public void Draw(int selectedOption, IEnumerable<string> values, Action<int> onUpdateCallback)
    {
        this.selectedOption = selectedOption;
        onUpdateAction = onUpdateCallback;

        options.Draw(values, (ui, pos, v) =>
        {
            ui.Label = v;
            ui.RegisterClick(() => SelectOption(pos));
        });
    }

    private void SelectOption(int pos)
    {
        openTask?.SetResult();

        selectedOption = pos;
        onUpdateAction?.Invoke(pos);

        for (int i = 0; i < options.getElementCount; i++)
        {
            if (i == pos)
                options[i].Classes.Add("Primary");
            else
                options[i].Classes.Remove("Primary");
        }
    }

    public Task Show()
    {
        openTask = new TaskCompletionSource();
        return openTask.Task;
    }
}