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
    private int? pos;
    private Func<int, Task>? onSelect;

    private ReusableList<CollectionItem> tags;

    public string LastOpened { get; set; } = "Never";

    public ImageCard()
    {
        InitializeComponent();
        tags = new ReusableList<CollectionItem>(cont_Tags);

        PointerPressed += HandleOnSelect;
    }

    public async Task Draw(ProjectCard info, int pos, Func<int, Task> onClick)
    {
        this.DataContext = info;

        this.pos = pos;
        this.onSelect = onClick;

        this.LastOpened = "Never";

        await tags.DrawAsync(() => DependencyManager.GetService<ITaggingLogic>()!.MapTags(info.tags), (ui, _, dat) =>
        {
            ui.Init(dat);
        });

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
            root.Classes.Add("Selected");
        else
            root.Classes.Remove("Selected");
    }
}