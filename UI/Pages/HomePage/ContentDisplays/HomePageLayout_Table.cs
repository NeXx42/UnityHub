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
    private StackPanel decor;
    private ButtonWrapper loadMoreBtn;

    private int currentPage = 1;
    public override int getTake => currentPage * 24;

    public HomePageLayout_Table(Panel scroller, Action<int> pageChange) : base(scroller, pageChange)
    {
        scroller.Children.Remove(container);

        decor = new StackPanel()
        {
            Spacing = 5
        };

        decor.Children.Add(new Border
        {
            CornerRadius = new Avalonia.CornerRadius(15),
            ClipToBounds = true,
            Background = MainWindow.instance!.FindResource("ButtonBorder") as IBrush,
            MinHeight = 0,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,

            Child = new Border()
            {
                CornerRadius = new Avalonia.CornerRadius(14),
                ClipToBounds = true,
                Margin = new Avalonia.Thickness(1),

                Child = container
            }
        });

        loadMoreBtn = new ButtonWrapper
        {
            Label = "Load more",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        loadMoreBtn.Classes.Add("Primary");
        loadMoreBtn.RegisterClick(RequestMore);

        decor.Children.Add(loadMoreBtn);
        scroller.Children.Add(decor);
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

    public override async Task Draw(ProjectInfo[] cardInfo, bool isPageIncrement, int _, int resultCount, Func<int, Task> onSelect)
    {
        if (!isPageIncrement)
            currentPage = 1;

        loadMoreBtn.IsVisible = resultCount > getTake;
        await cards.DrawWhenAll(cardInfo, (c, i, dat) => c.Draw(dat, i, onSelect));
    }

    protected override void ToggleElementSelection(TableCard element, bool to) => element.ToggleSelection(to);

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
                Data = Geometry.Parse("M0 2a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2zm15 2h-4v3h4zm0 4h-4v3h4zm0 4h-4v3h3a1 1 0 0 0 1-1zm-5 3v-3H6v3zm-5 0v-3H1v2a1 1 0 0 0 1 1zm-4-4h4V8H1zm0-4h4V4H1zm5-3v3h4V4zm4 4H6v3h4z")
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