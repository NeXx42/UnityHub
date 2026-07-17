using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Models.Data;

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
}