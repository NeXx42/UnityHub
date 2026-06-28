using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Models.Data;

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

    public void Draw(ProjectCard info, int pos, Func<int, Task> onClick)
    {
        this.pos = pos;
        this.onSelect = onClick;

        lbl_Name.Content = info.name;
        lbl_Location.Content = info.directory;
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