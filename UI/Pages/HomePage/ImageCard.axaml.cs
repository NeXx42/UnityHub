using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Logic;
using Microsoft.VisualBasic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;
using UI.Interfaces;

namespace UI.Pages.HomePage;

public interface IImageCardPlugin : IFrontendPlugin
{
    public void Setup(ImageCard card);
    public Task Draw(ImageCard card, ProjectInfo info, int pos, Func<int, Task> onClick);
}

public partial class ImageCard : UserControl
{
    public static FrontendPluginHandler<IImageCardPlugin> plugin = new();

    private bool registered = false;

    private int? pos;
    private ProjectInfo? activeCard;

    private Func<int, Task>? onSelect;

    private ReusableList<CollectionItem> tags;

    public Grid get_cont_Bottom => cont_Bottom;

    public ImageCard()
    {
        InitializeComponent();

        btn_ToggleFav.PointerPressed += ToggleFav;

        tags = new ReusableList<CollectionItem>(cont_Tags);
        plugin.Execute(t => t.Setup(this));

        PointerPressed += HandleOnSelect;
    }

    public async Task Draw(ProjectInfo info, int pos, Func<int, Task> onClick)
    {
        if (!registered)
        {
            registered = true;
            DependencyManager.GetService<ITaggingLogic>()!.RegisterCallback(UpdateTagging);
        }

        ToggleSelection(false);

        this.activeCard = info;
        this.DataContext = info;

        this.pos = pos;
        this.onSelect = onClick;

        UpdateFavStatus();
        await DrawTags();

        cont_Version.Classes.RemoveRange(0, cont_Version.Classes.Count);

        if (!await DependencyManager.GetService<IEditorLogic>()!.IsVersionInstalled(info.version))
            cont_Version.Classes.Add("Missing");

        await plugin.Execute(t => t.Draw(this, info, pos, onClick));
        await IconFetcher.GetImage(info.iconUrl, UpdateIcon);
    }

    private void UpdateIcon(string? path, Bitmap? icon)
    {
        img.Source = icon;
    }

    private void HandleOnSelect(object? sender, PointerEventArgs args)
    {
        if (!pos.HasValue || onSelect == null)
            return;

        onSelect(pos.Value).Wrap();
    }

    public void ToggleSelection(bool to)
    {
        if (to)
            border_Main.Classes.Add("Selected");
        else
            border_Main.Classes.Remove("Selected");

    }

    private void UpdateTagging(int? projectId, string msg)
    {
        if (!projectId.HasValue || activeCard == null || activeCard.id != projectId)
            return;

        DrawTags().Wrap();
    }

    private async Task DrawTags()
    {
        await tags.DrawAsync(() => DependencyManager.GetService<ITaggingLogic>()!.MapTags(activeCard!.tags), (ui, _, dat) =>
        {
            ui.Init(dat);
        }, 3);
    }

    private void ToggleFav(object? _, PointerEventArgs args)
    {
        if (activeCard == null)
            return;

        args.Handled = true;

        activeCard.favourited = !activeCard.favourited;
        DependencyManager.GetService<IProjectLogic>()!.UpdateProperties(activeCard, [nameof(ProjectInfo.favourited)]).Wrap();

        UpdateFavStatus();
    }

    private void UpdateFavStatus()
    {
        icon_ToggleFavToOff.IsVisible = activeCard!.favourited;
        icon_ToggleFavToOn.IsVisible = !activeCard!.favourited;
    }
}