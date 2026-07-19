using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Models.Data;
using UI.Controls;
using UI.Helpers;

namespace UI.Pages.HomePage.ContentDisplays;

public class HomePageLayout_Table : HomePageLayoutBase<TableCard>
{
    private Border decor;

    public HomePageLayout_Table(Panel scroller) : base(scroller)
    {
        decor = new Border
        {
            CornerRadius = new Avalonia.CornerRadius(15),
            ClipToBounds = true,
            Background = MainWindow.instance!.FindResource("ButtonBorder") as IBrush,
            MinHeight = 0,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
        };

        Border innerDecor = new Border()
        {
            CornerRadius = new Avalonia.CornerRadius(14),
            ClipToBounds = true,
            Margin = new Avalonia.Thickness(1)
        };

        scroller.Children.Remove(container);
        scroller.Children.Add(decor);

        innerDecor.Child = container;
        decor.Child = innerDecor;
    }

    protected override Panel GetWrapper()
    {
        StackPanel container = new StackPanel
        {
            Spacing = 1,
            Orientation = Avalonia.Layout.Orientation.Vertical,
        };

        return container;
    }

    public override void ToggleVisibility(bool to)
    {
        decor.IsVisible = to;
        base.ToggleVisibility(to);
    }

    public override async Task Draw(ProjectInfo[] cardInfo, Func<int, Task> onSelect)
    {
        await cards.DrawWhenAll(cardInfo, (c, i, dat) => c.Draw(dat, i, onSelect));
    }

    protected override void ToggleElementSelection(TableCard element, bool to) => element.ToggleSelection(to);

    public override ButtonWrapper CreateButton()
    {
        ButtonWrapper btn = base.CreateButton();
        btn.Label = string.Empty;
        btn.Icon = new Viewbox()
        {
            Height = 18,
            Child = new Path()
            {
                Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Data = Geometry.Parse("M0 2a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2zm15 2h-4v3h4zm0 4h-4v3h4zm0 4h-4v3h3a1 1 0 0 0 1-1zm-5 3v-3H6v3zm-5 0v-3H1v2a1 1 0 0 0 1 1zm-4-4h4V8H1zm0-4h4V4H1zm5-3v3h4V4zm4 4H6v3h4z")
            }
        };

        return btn;
    }
}