using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using Models.Interfaces;
using UI.Controls;
using UI.Helpers;
using UI.Interfaces;

namespace UI.Pages.HomePage;

public interface ITableCardPlugin : IFrontendPlugin
{
    public void Setup(TableCard card);
    public Task Draw(TableCard card, ProjectInfo info, int pos, Func<int, Task> onClick);
}

public partial class TableCard : UserControl
{
    public static FrontendPluginHandler<ITableCardPlugin> plugin = new();

    private int? pos;
    private ProjectInfo? activeCard;

    private Func<int, Task>? onSelect;

    public TableCard()
    {
        InitializeComponent();

        this.DataContext = ProjectInfo.Test;
        btn_ToggleFav.PointerPressed += ToggleFav;

        plugin.Execute(t => t.Setup(this));

        PointerPressed += HandleOnSelect;
    }

    public async Task Draw(ProjectInfo info, int pos, Func<int, Task> onClick)
    {
        ToggleSelection(false);

        this.activeCard = info;
        this.DataContext = info;

        this.pos = pos;
        this.onSelect = onClick;

        UpdateFavStatus();

        cont_Version.Classes.RemoveRange(0, cont_Version.Classes.Count);

        if (!await DependencyManager.GetService<IEditorLogic>()!.IsVersionInstalled(info.version))
            cont_Version.Classes.Add("Missing");

        await plugin.Execute(t => t.Draw(this, info, pos, onClick));
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