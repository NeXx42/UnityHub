using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.Controls;
using UI.Helpers;
using UI.Interfaces;

namespace UI.Popups;

public partial class Popup_GenericList : UserControl, IPopup
{
    private TaskCompletionSource? openTask;

    private Func<int, string, Task>? callback;

    private Func<Task<string[]>>? dataloader;
    private ReusableList<ButtonWrapper> buttons;

    public Popup_GenericList()
    {
        InitializeComponent();
        buttons = new ReusableList<ButtonWrapper>(cont, () =>
        {
            ButtonWrapper btn = new ButtonWrapper();
            btn.Classes.Add("Transparent");

            return btn;
        });
    }

    public void Draw(Func<Task<string[]>> loader, Func<int, string, Task> onCallback)
    {
        this.callback = onCallback;
        this.dataloader = loader;
    }

    public void Draw(string[] options, Func<int, string, Task> onCallback)
    {
        callback = onCallback;
        buttons.Draw(options, DrawElement);
    }

    public Task Show()
    {
        openTask?.SetCanceled();
        openTask = new TaskCompletionSource();

        if (dataloader != null)
            Redraw().Wrap();

        return openTask.Task;

        async Task Redraw()
        {

            await dataloader();
            await buttons.DrawAsync(dataloader, DrawElement);
        }
    }

    private void DrawElement(ButtonWrapper ui, int id, string dat)
    {
        ui.Label = dat;
        ui.RegisterClick(Complete);

        async Task Complete()
        {
            openTask?.SetResult();
            await (callback?.Invoke(id, dat) ?? Task.CompletedTask);
        }
    }
}