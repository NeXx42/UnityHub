using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Models.Data;

namespace UI.Pages.HomePage.ContentDisplays;

public class HomePageLayout_Grid : HomePageLayoutBase<ImageCard>
{
    public HomePageLayout_Grid(Panel scroller) : base(scroller) { }

    protected override Panel GetWrapper()
    {
        WrapPanel container = new WrapPanel();
        container.ItemSpacing = 5;
        container.LineSpacing = 5;

        return container;
    }

    public override async Task Draw(ProjectInfo[] cardInfo, Func<int, Task> onSelect)
    {
        await cards.DrawWhenAll(cardInfo, (c, i, dat) => c.Draw(dat, i, onSelect));
    }

    protected override void ToggleElementSelection(ImageCard element, bool to) => element.ToggleSelection(to);
}
