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
    private TaskCompletionSource<TagData?>? saveTask;

    public CreateCollectionModal()
    {
        InitializeComponent();
        btn.Click += (_, __) => Save();
    }

    public bool canDismiss => true;
    public ModalContainer setContainer { set => container = value; }

    public Task<TagData?> Init()
    {
        saveTask = new TaskCompletionSource<TagData?>();
        return saveTask.Task;
    }

    private void Save()
    {
        if (saveTask == null || string.IsNullOrEmpty(txt.Text))
            return;

        saveTask.SetResult(new TagData()
        {
            collectionId = -1,
            collectionName = txt.Text,
        });

        container?.requestCloserEvent?.Invoke();
    }
}