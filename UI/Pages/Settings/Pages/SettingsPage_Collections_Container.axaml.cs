using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Modals;
using UI.Popups;

namespace UI.Pages.Settings.Pages;

public partial class SettingsPage_Collections_Container : UserControl
{
    private TagData? activeData;

    public SettingsPage_Collections_Container()
    {
        InitializeComponent();

        Popup_GenericList options = new Popup_GenericList();
        options.Draw(["Edit", "Delete"], OnOpen);

        btn_Extra.RegisterPopup(options);
    }

    public void Draw<T>(T dat, int index) where T : TagData
    {
        if (index % 2 == 0)
            cont.Classes.Remove("Odd");
        else
            cont.Classes.Add("Odd");

        this.activeData = dat;
        this.DataContext = dat;

        btn_Extra.IsVisible = true;

        if (activeData is CollectionData colDat)
            btn_Extra.IsVisible = !colDat.isDefault;

        cont_CollectionResult.Init(dat);
    }

    private async Task OnOpen(int _, string option)
    {
        ITaggingLogic logic = DependencyManager.GetService<ITaggingLogic>()!;

        switch (option)
        {
            case "Edit":
                TagData? res = await MainWindow.ShowModalAndWait<CreateCollectionModal, TagData>(async d => await d.Init(activeData));

                if (res == null || res.collectionId == -1)
                    return;

                if (res is CollectionData resCol)
                    await logic.CreateOrUpdateCollection(resCol);
                else
                    await logic.CreateOrUpdateTag(res);

                break;

            case "Delete":
                if (activeData == null)
                    return;

                if (activeData is CollectionData)
                {
                    if (await DependencyManager.ui!.ShowConfirmationBox(
                        "Delete",
                        "Are you sure you want to remove this Collection? Any project using it will have its collection reverted to \"In Development\".",
                        new ConfirmationButton("Cancel"),
                        new ConfirmationButton("Delete", true)
                    ) == 1)
                        await logic.DeleteCollection(activeData.collectionId);
                }
                else
                {
                    if (await DependencyManager.ui!.ShowConfirmationBox(
                        "Delete",
                        "Are you sure you want to delete this tag? Any project using the tag will lose it.",
                        new ConfirmationButton("Cancel"),
                        new ConfirmationButton("Delete", true)
                    ) == 1)
                        await logic.DeleteTag(activeData.collectionId);
                }
                break;
        }
    }
}