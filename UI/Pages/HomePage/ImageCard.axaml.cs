using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;

namespace UI.Pages.HomePage;

public partial class ImageCard : UserControl
{
    private bool registered = false;

    private int? pos;
    private ProjectInfo? activeCard;

    private Func<int, Task>? onSelect;

    private ReusableList<CollectionItem> tags;

    public string LastOpened { get; set; } = "Never";

    public ImageCard()
    {
        InitializeComponent();
        tags = new ReusableList<CollectionItem>(cont_Tags);

        PointerPressed += HandleOnSelect;
    }

    public async Task Draw(ProjectInfo info, int pos, Func<int, Task> onClick)
    {
        if (!registered)
        {
            registered = true;
            DependencyManager.GetService<ITaggingLogic>()!.RegisterCallback(UpdateTagging);
        }

        this.activeCard = info;
        this.DataContext = info;

        this.pos = pos;
        this.onSelect = onClick;

        this.LastOpened = "Never";

        await DrawTags();

        cont_Version.Classes.RemoveRange(0, cont_Version.Classes.Count);

        if (!DependencyManager.GetService<IEditorLogic>()!.IsVersionInstalled(info.version))
            cont_Version.Classes.Add("Missing");

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
}