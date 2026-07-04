using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;
using UI.Controls;
using UI.Helpers;

namespace UI.Popups;

public partial class Popup_Collection : UserControl
{
    private ReusableList<CollectionItem> items;

    public Popup_Collection()
    {
        InitializeComponent();
        items = new ReusableList<CollectionItem>(Entries);
    }

    public async Task<Popup_Collection> Init(Func<Task<CollectionData[]>> fetchTask, Func<CollectionData, Task> onSelectEntry)
    {
        await items.DrawAsync(fetchTask, (ui, _, dat) =>
        {
            ui.Init(dat, () => onSelectEntry(dat));
        });

        return this;
    }
}