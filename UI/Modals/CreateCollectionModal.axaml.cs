using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Models.Data;
using UI.Controls;

namespace UI.Modals;

public partial class CreateCollectionModal : UserControl, IModal
{
    private ModalContainer? container;
    private TaskCompletionSource<CollectionData?>? saveTask;

    public CreateCollectionModal()
    {
        InitializeComponent();
        btn.Click += (_, __) => Save();
    }

    public bool canDismiss => true;
    public ModalContainer setContainer { set => container = value; }

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
            collectionName = txt.Text,
            type = ""
        });

        container?.requestCloserEvent?.Invoke();
    }
}