using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Logic;
using Models.Data;
using Models.Enums;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;

namespace UI.Modals;

public struct CreateCollectionModal_Colour
{
    public string colourName { get; set; }
    public IBrush colour { get; set; }
}

public partial class CreateCollectionModal : UserControl, IModal, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged;

    private int? representingId { get; set; }
    public bool servingCollection { get; set; }

    private TaskCompletionSource<TagData>? saveTask;

    public CreateCollectionModal()
    {
        InitializeComponent();

        btn_Cancel.RegisterClick(() => saveTask?.SetCanceled());
        btn_Save.RegisterClick(Save);

        inp_CollectionHandling.ItemsSource = Enum.GetValues<CollectionHandlingTypes>();
    }

    public bool canDismiss => true;
    public ModalContainer setContainer { set => _ = value; }

    public Task<TagData> Init<T>(T? existingData) where T : TagData
    {
        saveTask = new TaskCompletionSource<TagData>();
        representingId = existingData?.collectionId;

        int collectionSize;

        if (typeof(T) == typeof(CollectionData) || existingData is CollectionData)
        {
            servingCollection = true;
            collectionSize = DependencyManager.GetService<ITaggingLogic>()!.GetCollectionCount();

            if (existingData is CollectionData colDat)
            {
                inp_CollectionHandling.SelectedIndex = (int)colDat.handlingType;
            }
            else
            {
                inp_CollectionHandling.SelectedIndex = 0;
            }
        }
        else
        {
            servingCollection = false;
            collectionSize = DependencyManager.GetService<ITaggingLogic>()!.GetTagCount();
        }


        int? selectedColourIndex = null;
        string[] colours = ThemeHelper.collectionColours;

        if (existingData != null)
        {
            for (int i = 0; i < colours.Length; i++)
            {
                if (colours[i].Equals(existingData.colour, StringComparison.InvariantCultureIgnoreCase))
                {
                    selectedColourIndex = i;
                    break;
                }
            }

            if (!selectedColourIndex.HasValue && !string.IsNullOrEmpty(existingData.colour))
                colours = [existingData.colour, .. colours];
        }

        inp_Colours.ItemsSource = colours.Select(c => new CreateCollectionModal_Colour() { colour = new SolidColorBrush(Color.Parse(c)), colourName = c });
        inp_Colours.SelectedIndex = selectedColourIndex ?? (collectionSize % colours.Length);
        inp_Name.Text = existingData?.collectionName;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));

        return saveTask.Task;
    }

    private void Save()
    {
        CreateCollectionModal_Colour? colour = inp_Colours.SelectedItem as CreateCollectionModal_Colour?;

        if (saveTask == null || string.IsNullOrEmpty(inp_Name.Text) || colour == null)
            return;

        if (servingCollection)
        {
            saveTask.SetResult(new CollectionData()
            {
                collectionId = representingId ?? -1,
                collectionName = inp_Name.Text,
                colour = colour.Value.colourName,
                handlingType = (CollectionHandlingTypes)inp_CollectionHandling.SelectedItem!
            });
        }
        else
        {
            saveTask.SetResult(new TagData()
            {
                collectionId = representingId ?? -1,
                collectionName = inp_Name.Text,
                colour = colour.Value.colourName
            });
        }
    }
}