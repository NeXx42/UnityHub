using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;

namespace UI.Modals;

public partial class CreateCollectionModal : UserControl
{
    private TaskCompletionSource<CollectionData?>? saveTask;

    public CreateCollectionModal()
    {
        InitializeComponent();
        btn.Click += (_, __) => Save();
    }

    public Task<CollectionData?> Init()
    {
        saveTask = new TaskCompletionSource<CollectionData?>();
        return saveTask.Task;
    }

    private void Save()
    {
        if (saveTask == null || string.IsNullOrEmpty(txt.Text))
            return;

        saveTask.SetResult(new CollectionData()
        {
            collectionId = -1,
            collectionName = txt.Text
        });
    }
}