using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;

namespace UI.Controls;

public partial class CollectionItem : UserControl
{
    public CollectionData? collection;

    public CollectionItem()
    {
        InitializeComponent();
        DataContext = new CollectionData() { collectionId = 0, collectionName = "test" };
    }

    public void Init(CollectionData collection)
    {
        DataContext = collection;
    }
}