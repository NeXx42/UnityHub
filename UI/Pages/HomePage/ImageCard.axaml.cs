using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Helpers;

namespace UI.Pages.HomePage;

public partial class ImageCard : UserControl
{
    private int? pos;
    private Func<int, Task>? onSelect;

    public ImageCard()
    {
        InitializeComponent();
        PointerPressed += HandleOnSelect;
    }

    public async Task Draw(ProjectCard info, int pos, Func<int, Task> onClick)
    {
        this.DataContext = info;

        this.pos = pos;
        this.onSelect = onClick;

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

        _ = onSelect(pos.Value);
    }

    public void ToggleSelection(bool to)
    {
        Classes.RemoveRange(0, Classes.Count);

        if (to)
        {
            Classes.Add("Selected");
        }
    }
}