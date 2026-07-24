using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;
using UI.Popups;

namespace UI.Pages.HomePage;

public partial class MoreInfo : UserControl
{
    public ProjectInfo? info { get; private set; }

    private ReusableList<CollectionItem> tags;
    private ReusableList<CollectionItem> collections;

    public MoreInfo()
    {
        InitializeComponent();

        tags = new ReusableList<CollectionItem>(cont_Tags);
        collections = new ReusableList<CollectionItem>(cont_Collections);

        btn_OpenProject.RegisterClick(() => DependencyManager.GetService<IEditorLogic>()!.LaunchProject(info!));
        btn_OpenProject.RegisterOptions(["Rederive Metadata", "Upload Icon"], OnLaunchOptionSelect);

        btn_OpenIDE.RegisterClick(() => DependencyManager.GetService<IProjectLogic>()!.OpenIDE(info!));

        btn_Terminal.RegisterClick(() => DependencyManager.GetService<IProjectLogic>()!.BrowseTerminal(info!));
        btn_Move.RegisterClick(() => DependencyManager.GetService<IProjectLogic>()!.MoveProject(info!));
        btn_Clone.RegisterClick(() => DependencyManager.GetService<IProjectLogic>()!.DuplicateProject(info!));
        btn_Delete.RegisterClick(DeleteProject);

        Popup_GenericList versionList = new Popup_GenericList();
        versionList.Draw(GetEditorVersions, SelectNewEditorVersion);
        inp_Version.RegisterPopup(versionList);

        inp_Notes.TextChanged += (_, __) => btn_SaveNotes.IsVisible = !(inp_Notes.Text ?? "").Equals(info?.notes ?? "");
        btn_SaveNotes.RegisterClick(SaveNotes);

        if (!Design.IsDesignMode)
        {
            cont_Main.IsVisible = false;
            cont_Message.IsVisible = true;
        }
    }

    public async Task Show(int? id)
    {
        if (info?.id == id)
            return;

        MainWindow.ClearFocus();

        info = await DependencyManager.GetService<IProjectLogic>()!.GetProjectInfo(id);
        DataContext = info;

        cont_Message.IsVisible = info == null;
        cont_Main.IsVisible = info != null;

        if (info == null)
            return;

        inp_Notes.Text = info.notes;
        btn_SaveNotes.IsVisible = false;

        ITaggingLogic logic = DependencyManager.GetService<ITaggingLogic>()!;

        await Task.WhenAll([
            RedrawTags(logic),
            RedrawCollections(logic),
        ]);

        img.Source = await IconFetcher.GetImage(info.iconUrl);

        btn_AddTag.RegisterPopup(await new Popup_Collection().Init(
            logic.GetTags,
            AddTag,
            () => btn_AddTag.IsOpen = false)
        );
        btn_AddCollection.RegisterPopup(await new Popup_Collection().Init(
            logic.GetCollections,
            ChangeCollection,
            () => btn_AddCollection.IsOpen = false)
        );
    }

    private async Task AddTag(TagData data)
    {
        if (info == null || info.tags.Contains(data.collectionId))
            return;

        info.tags.Add(data.collectionId);

        ITaggingLogic logic = DependencyManager.GetService<ITaggingLogic>()!;
        await logic.UpdateTag(info.id, data.collectionId, true);
        await RedrawTags(logic);
    }

    private async Task ChangeCollection(TagData data)
    {
        if (info == null)
            return;

        ITaggingLogic logic = DependencyManager.GetService<ITaggingLogic>()!;

        if (await logic.TryToChangeCollection(info, data.collectionId))
            await RedrawCollections(logic);
    }

    private async Task RedrawTags(ITaggingLogic logic)
    {
        await tags.DrawAsync(() => logic.MapTags(info?.tags ?? []), (ui, _, dat) => ui.Init(dat, null, () => RemoveCollection(dat)));

        async Task RemoveCollection(TagData dat)
        {
            info!.tags.Remove(dat.collectionId);

            await logic.UpdateTag(info.id, dat.collectionId, false);
            await RedrawTags(logic);
        }
    }

    private async Task RedrawCollections(ITaggingLogic logic)
    {
        await collections.DrawAsync(() => logic.MapCollections([info!.collectionId]), (ui, _, dat) => ui.Init(dat, null));
    }

    private async Task DeleteProject()
    {
        if (info == null)
            return;

        if (await MainWindow.instance!.ShowConfirmationBox("Delete Project", $"Are you sure you want to delete the project\n'{info.name}'?",
            new ConfirmationButton()
            {
                label = "Cancel",
            },
            new ConfirmationButton()
            {
                className = "Primary",
                label = "Delete",
            }) != 1
        )
            return;

        await DependencyManager.GetService<IProjectLogic>()!.DeleteCard(info);
    }

    private async Task OnLaunchOptionSelect(int id)
    {
        switch (id)
        {
            case 0: // rederive
                await DependencyManager.ui!.LoadProgressive("Deriving", DependencyManager.GetService<IProjectLogic>()!.DeriveProjectInfo(info!, true));
                break;
        }
    }

    private async Task<string[]> GetEditorVersions()
    {
        string[] versions = (await DependencyManager.GetService<IEditorLogic>()!.GetInstalledEditorVersions()).OrderDescending().ToArray();
        return versions;
    }

    private async Task SelectNewEditorVersion(int _, string val)
    {
        if (info == null)
            return;

        IProjectLogic logic = DependencyManager.GetService<IProjectLogic>()!;
        await logic.TrySwitchVersion(info, val);
    }

    private async Task SaveNotes()
    {
        if (info == null)
            return;

        info.notes = inp_Notes.Text;
        btn_SaveNotes.IsVisible = false;

        IProjectLogic logic = DependencyManager.GetService<IProjectLogic>()!;
        await logic.UpdateProperties(info, [nameof(ProjectInfo.notes)]);
    }
}