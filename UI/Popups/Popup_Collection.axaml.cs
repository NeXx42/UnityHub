using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;
using UI.Interfaces;
using UI.Modals;

namespace UI.Popups;

public partial class Popup_Collection : UserControl, IPopup
{
    private Action? closer;
    private Func<Task>? drawer;

    private ReusableList<CollectionItem> items;

    public string CollectionName { get; set; } = "Tag";

    public Popup_Collection()
    {
        InitializeComponent();

        items = new ReusableList<CollectionItem>(Entries);
    }

    public async Task<Popup_Collection> Init<T>(Func<Task<T[]>> fetchTask, Func<T, Task> onSelectEntry, Action closer) where T : TagData
    {
        drawer = Draw;

        this.closer = closer;
        await drawer.Invoke();

        async Task Draw()
        {
            await items.DrawAsync(fetchTask, (ui, _, dat) =>
            {
                ui.Init(dat, async () =>
                {
                    this.closer?.Invoke();
                    await onSelectEntry(dat);
                });
            });
        }

        return this;
    }

    public Task Show()
    {
        TaskCompletionSource task = new TaskCompletionSource();
        return task.Task;
    }
}