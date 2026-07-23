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

    public TagData? collection;

    public CollectionItem()
    {
        InitializeComponent();
        DataContext = new TagData() { collectionId = 0, collectionName = "temp" };

        btn_Remove.PointerPressed += (_, e) =>
        {
            e.Handled = true;
            onRemoveCallback?.Invoke();
        };

        this.PointerPressed += (_, __) => onClickCallback?.Invoke();
    }

    public void Init(TagData collection, Func<Task>? onClick = null, Func<Task>? onRemove = null)
    {
        if (string.IsNullOrEmpty(collection.tooltip))
            ToolTip.SetTip(this, null);
        else
            ToolTip.SetTip(this, collection.tooltip);

        DataContext = collection;
        onClickCallback = onClick.Wrap;
        onRemoveCallback = onRemove.Wrap;

        ShowCloseButton = onRemove != null;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowCloseButton)));
    }
}