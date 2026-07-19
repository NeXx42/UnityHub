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
    private Func<TagData, Task>? saver;

    private ReusableList<CollectionItem> items;

    public string CollectionName { get; set; } = "Tag";

    public Popup_Collection()
    {
        InitializeComponent();

        items = new ReusableList<CollectionItem>(Entries);
        btn_Add.RegisterClick(CreateCollection);
    }

    public async Task<Popup_Collection> Init<T>(Func<Task<T[]>> fetchTask, Func<T, Task> onSelectEntry, Func<TagData, Task> addCollection, Action closer) where T : TagData
    {
        drawer = Draw;
        saver = addCollection;

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

    private async Task CreateCollection()
    {
        this.closer?.Invoke();

        CreateCollectionModal creator = MainWindow.ShowModal<CreateCollectionModal>(out int pos);
        TagData? dat = await creator.Init();

        await MainWindow.CloseModal(pos);

        if (dat != null && saver != null)
        {
            await saver(dat);
            await drawer!();
        }
    }

    public Task Show()
    {
        return Task.CompletedTask;
    }
}