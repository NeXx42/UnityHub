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
    private bool servingCollection { get; set; }

    private ModalContainer? container;
    private TaskCompletionSource<TagData>? saveTask;

    public CreateCollectionModal()
    {
        InitializeComponent();

        btn_Cancel.RegisterClick(() => saveTask?.SetCanceled());
        btn_Save.RegisterClick(Save);
    }

    public bool canDismiss => true;
    public ModalContainer setContainer { set => container = value; }

    public Task<TagData> Init<T>(T? existingData) where T : TagData
    {
        saveTask = new TaskCompletionSource<TagData>();

        inp_Name.Text = existingData?.collectionName;
        inp_Colour.Text = existingData?.colour;

        if (typeof(T) == typeof(CollectionData) || existingData is CollectionData)
        {
            servingCollection = true;

            if (existingData is CollectionData colDat)
            {

            }
        }
        else
        {
            servingCollection = false;
        }

        return saveTask.Task;
    }

    private void Save()
    {
        if (saveTask == null || string.IsNullOrEmpty(inp_Name.Text))
            return;

        if (servingCollection)
        {
            saveTask.SetResult(new CollectionData()
            {
                collectionId = -1,
                collectionName = inp_Name.Text,
            });
        }
        else
        {
            saveTask.SetResult(new TagData()
            {
                collectionId = -1,
                collectionName = inp_Name.Text,
            });
        }
    }
}