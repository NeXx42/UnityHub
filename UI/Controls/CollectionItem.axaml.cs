using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;
using UI.Helpers;

namespace UI.Controls;

public partial class CollectionItem : UserControl
{
    private Action? onClickCallback;
    public CollectionData? collection;

    public CollectionItem()
    {
        InitializeComponent();
        DataContext = new CollectionData() { collectionId = 0, collectionName = "test" };

        this.PointerPressed += (_, __) => onClickCallback?.Invoke();
    }

    public void Init(CollectionData collection, Func<Task>? onClick = null)
    {
        DataContext = collection;
        onClickCallback = onClick.Wrap;
    }
}