using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Models.Data;
using UI.Controls;

namespace UI.Pages.HomePage.ContentDisplays;

public class HomePageLayout_List : HomePageLayoutBase<ListCard>
{
    public HomePageLayout_List(Panel scroller) : base(scroller)
    {

    }

    protected override Panel GetWrapper()
    {
        StackPanel container = new StackPanel();
        container.Spacing = 5;
        container.Orientation = Avalonia.Layout.Orientation.Vertical;

        return container;
    }

    public override async Task Draw(ProjectInfo[] cardInfo, Func<int, Task> onSelect)
    {
        await cards.DrawWhenAll(cardInfo, (c, i, dat) => c.Draw(dat, i, onSelect));
    }

    protected override void ToggleElementSelection(ListCard element, bool to) => element.ToggleSelection(to);

    public override ButtonWrapper CreateButton()
    {
        ButtonWrapper btn = base.CreateButton();
        btn.Classes.Add("Transparent");
        btn.Label = string.Empty;
        btn.Icon = new Viewbox()
        {
            Height = 18,
            Child = new Path()
            {
                Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Data = Geometry.Parse("M2.5 12a.5.5 0 0 1 .5-.5h10a.5.5 0 0 1 0 1H3a.5.5 0 0 1-.5-.5m0-4a.5.5 0 0 1 .5-.5h10a.5.5 0 0 1 0 1H3a.5.5 0 0 1-.5-.5m0-4a.5.5 0 0 1 .5-.5h10a.5.5 0 0 1 0 1H3a.5.5 0 0 1-.5-.5")
            }
        };

        return btn;
    }
}