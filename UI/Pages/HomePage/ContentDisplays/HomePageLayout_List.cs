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
    private StackPanel wrapper;
    private ButtonWrapper loadMoreBtn;

    private int currentPage = 1;
    public override int getTake => currentPage * 9;

    public HomePageLayout_List(Panel scroller, Action<int> pageChange) : base(scroller, pageChange)
    {
        scroller.Children.Remove(container);

        wrapper = new StackPanel()
        {
            Spacing = 5
        };

        wrapper.Children.Add(container);

        loadMoreBtn = new ButtonWrapper
        {
            Label = "Load more",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        loadMoreBtn.Classes.Add("Primary");
        loadMoreBtn.RegisterClick(RequestMore);

        wrapper.Children.Add(loadMoreBtn);
        scroller.Children.Add(wrapper);
    }

    protected override Panel GetWrapper()
    {
        StackPanel container = new StackPanel();
        container.Spacing = 5;
        container.Orientation = Avalonia.Layout.Orientation.Vertical;

        return container;
    }

    public override void ToggleVisibility(bool to)
    {
        wrapper.IsVisible = to;
        base.ToggleVisibility(to);
    }

    public override async Task Draw(ProjectInfo[] cardInfo, bool isPageIncrement, int _, int resultCount, Func<int, Task> onSelect)
    {
        if (!isPageIncrement)
            currentPage = 1;

        loadMoreBtn.IsVisible = resultCount > getTake;
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

    private async Task RequestMore()
    {
        currentPage++;
        onPageChangeCallback?.Invoke(0);
    }
}