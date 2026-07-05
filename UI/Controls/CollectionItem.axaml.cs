using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;
using UI.Helpers;

namespace UI.Controls;

public partial class CollectionItem : UserControl, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged;

    public bool ShowCloseButton { get; set; } = false;

    private Action? onClickCallback;
    private Action? onRemoveCallback;

    public CollectionData? collection;

    public CollectionItem()
    {
        InitializeComponent();
        DataContext = new CollectionData() { collectionId = 0, collectionName = "temp", type = "temp" };

        btn_Remove.PointerPressed += (_, e) =>
        {
            e.Handled = true;
            onRemoveCallback?.Invoke();
        };

        this.PointerPressed += (_, __) => onClickCallback?.Invoke();
    }

    public void Init(CollectionData collection, Func<Task>? onClick = null, Func<Task>? onRemove = null)
    {
        DataContext = collection;
        onClickCallback = onClick.Wrap;
        onRemoveCallback = onRemove.Wrap;

        ShowCloseButton = onRemove != null;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowCloseButton)));
    }
}