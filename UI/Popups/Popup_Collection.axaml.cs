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
using UI.Modals;

namespace UI.Popups;

public partial class Popup_Collection : UserControl
{
    private Action? closer;
    private Func<Task>? drawer;
    private Func<CollectionData, Task>? saver;

    private ReusableList<CollectionItem> items;

    public string CollectionName { get; set; } = "Tag";

    public Popup_Collection()
    {
        InitializeComponent();

        items = new ReusableList<CollectionItem>(Entries);
        btn_Add.RegisterClick(CreateCollection);
    }

    public async Task<Popup_Collection> Init(Func<Task<CollectionData[]>> fetchTask, Func<CollectionData, Task> onSelectEntry, Func<CollectionData, Task> addCollection, Action closer)
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
        CollectionData? dat = await creator.Init();

        MainWindow.CloseModal(pos);

        if (dat != null && saver != null)
        {
            await saver(dat);
            await drawer!();
        }
    }
}