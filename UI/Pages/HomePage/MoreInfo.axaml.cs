using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;
using UI.Interfaces;
using UI.Modals;
using UI.Popups;

namespace UI.Pages.HomePage;

public interface MoreInfo_Plugin : IFrontendPlugin
{
    public void Setup(MoreInfo source);
    public void UpdateSelected(ProjectInfo? info);
}

public partial class MoreInfo : UserControl
{
    public ProjectInfo? info { get; private set; }

    private ReusableList<CollectionItem> tags;
    private ReusableList<CollectionItem> collections;

    public static FrontendPluginHandler<MoreInfo_Plugin> plugins = new FrontendPluginHandler<MoreInfo_Plugin>();

    public StackPanel getActionsContainer => cont_Actions;
    public StackPanel getContentContainer => cont_Main;
    public StackPanel getMetadataContentContainer => cont_MetaData;

    public MoreInfo()
    {
        InitializeComponent();

        tags = new ReusableList<CollectionItem>(cont_Tags);
        collections = new ReusableList<CollectionItem>(cont_Collections);

        btn_OpenProject.RegisterClick(() => DependencyManager.GetService<IEditorLogic>()!.LaunchProject(info!));
        btn_OpenProject.RegisterOptions(["Rederive Metadata", "Upload Icon"], OnLaunchOptionSelect);

        btn_OpenIDE.RegisterClick(() => DependencyManager.GetService<IProjectLogic>()!.OpenIDE(info!));
        btn_Terminal.RegisterClick(() => DependencyManager.GetService<IProjectLogic>()!.BrowseTerminal(info!));
        btn_OpenExplorer.RegisterClick(() => DependencyManager.GetService<IProjectLogic>()!.BrowseTo(info!));

        btn_Move.RegisterClick(MoveProject);
        btn_Clone.RegisterClick(CloneProject);
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

        plugins.Execute(p => p.Setup(this));
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
        {
            plugins.Execute(p => p.UpdateSelected(info));
            return;
        }

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

        plugins.Execute(p => p.UpdateSelected(info));
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

        if (await MainWindow.instance!.ShowConfirmationBox("Delete Project", $"Are you sure you want to delete the project\n'{info.name}'?", LanguageHelper.Button_Cancel,
            new ConfirmationButton()
            {
                className = "Primary",
                label = LanguageHelper.GetLanguageResource("Literal_Delete")!,
            }) != 1
        )
            return;

        LoadRequest[] deleteTasks = DependencyManager.GetService<IProjectLogic>()!.DeleteCard(info);
        Exception? e = await DependencyManager.ui!.LoadProgressive("Deleting", deleteTasks);

        if (e != null)
            await DependencyManager.ui!.ShowMessageBox(e);
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

    private async Task MoveProject()
    {
        if (info == null)
            return;

        await MainWindow.ShowModalAndWait<MoveProject_Modal>(async m => await m.Open(info));
    }

    private async Task CloneProject()
    {
        if (info == null)
            return;

        await MainWindow.ShowModalAndWait<DuplicateProject_Modal>(async m => await m.Open(info));
    }
}