using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Models.Data;
using UI.Controls;
using UI.Helpers;

namespace UI.Pages.HomePage.ContentDisplays;

public class HomePageLayout_Grid : HomePageLayoutBase<ImageCard>
{
    private const double ELEMENT_MIN_WIDTH = 280;
    private const double ELEMENT_ASPECT_RATIO = 0.8125;

    private StackPanel wrapper;
    private ReusableList<ButtonWrapper> pageControls;

    public HomePageLayout_Grid(Panel scroller, Action<int> pageChange) : base(scroller, pageChange)
    {
        scroller.Children.Remove(container);

        wrapper = new StackPanel
        {
            Spacing = 5
        };
        wrapper.Children.Add(container);

        StackPanel pageControlsContainer = new StackPanel()
        {
            Spacing = 5,
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        wrapper.Children.Add(pageControlsContainer);
        pageControls = new ReusableList<ButtonWrapper>(pageControlsContainer);

        scroller.Children.Add(wrapper);
    }

    protected override Panel GetWrapper()
    {
        UniformGrid container = new UniformGrid()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };

        container.SizeChanged += OnContainerSizeChange;
        return container;
    }

    public override void ToggleVisibility(bool to)
    {
        wrapper.IsVisible = to;
        base.ToggleVisibility(to);
    }

    public override async Task Draw(ProjectInfo[] cardInfo, bool isPageIncrement, int currentPage, int resultCount, Func<int, Task> onSelect)
    {
        DrawPageControls(currentPage, resultCount);
        cards.Draw(cardInfo, (c, i, dat) => c.Draw(dat, i, onSelect).Wrap());
    }

    protected override void ToggleElementSelection(ImageCard element, bool to) => element.ToggleSelection(to);

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
                Data = Geometry.Parse("M1 2.5A1.5 1.5 0 0 1 2.5 1h3A1.5 1.5 0 0 1 7 2.5v3A1.5 1.5 0 0 1 5.5 7h-3A1.5 1.5 0 0 1 1 5.5zM2.5 2a.5.5 0 0 0-.5.5v3a.5.5 0 0 0 .5.5h3a.5.5 0 0 0 .5-.5v-3a.5.5 0 0 0-.5-.5zm6.5.5A1.5 1.5 0 0 1 10.5 1h3A1.5 1.5 0 0 1 15 2.5v3A1.5 1.5 0 0 1 13.5 7h-3A1.5 1.5 0 0 1 9 5.5zm1.5-.5a.5.5 0 0 0-.5.5v3a.5.5 0 0 0 .5.5h3a.5.5 0 0 0 .5-.5v-3a.5.5 0 0 0-.5-.5zM1 10.5A1.5 1.5 0 0 1 2.5 9h3A1.5 1.5 0 0 1 7 10.5v3A1.5 1.5 0 0 1 5.5 15h-3A1.5 1.5 0 0 1 1 13.5zm1.5-.5a.5.5 0 0 0-.5.5v3a.5.5 0 0 0 .5.5h3a.5.5 0 0 0 .5-.5v-3a.5.5 0 0 0-.5-.5zm6.5.5A1.5 1.5 0 0 1 10.5 9h3a1.5 1.5 0 0 1 1.5 1.5v3a1.5 1.5 0 0 1-1.5 1.5h-3A1.5 1.5 0 0 1 9 13.5zm1.5-.5a.5.5 0 0 0-.5.5v3a.5.5 0 0 0 .5.5h3a.5.5 0 0 0 .5-.5v-3a.5.5 0 0 0-.5-.5z")
            }
        };

        return btn;
    }

    private void DrawPageControls(int currentPage, int resultCount)
    {
        if (resultCount < getTake)
        {
            pageControls.Clear();
            return;
        }

        const int MaxPageDistance = 4;
        List<int> pageOptions = new List<int>();

        int maxPage = (int)Math.Ceiling(resultCount / (float)getTake);

        for (int i = currentPage - MaxPageDistance; i < currentPage + MaxPageDistance; i++)
        {
            if (i >= 0 && i < maxPage)
                pageOptions.Add(i);
        }

        pageControls.Draw(pageOptions, (lbl, _, dat) =>
        {
            lbl.Label = (dat + 1).ToString();
            lbl.RegisterClick(() => UpdatePage(dat));

            if (dat == currentPage)
                lbl.Classes.Add("Primary");
            else
                lbl.Classes.Remove("Primary");
        });
    }

    private async Task UpdatePage(int to)
    {
        onPageChangeCallback?.Invoke(to);
    }

    private void OnContainerSizeChange(object? sender, SizeChangedEventArgs args)
    {
        int maxElements = (int)Math.Floor(args.NewSize.Width / ELEMENT_MIN_WIDTH);
        maxElements = Math.Max(maxElements, 1);

        (args.Source as UniformGrid)!.Columns = maxElements;

        const double spacing = 10;

        double columnWidth = args.NewSize.Width / maxElements;
        double widthPerElement = columnWidth - spacing;

        foreach (var card in cards)
        {
            card.Margin = new Avalonia.Thickness(spacing / 2, spacing / 2);
            card.Width = widthPerElement;
            card.Height = widthPerElement * ELEMENT_ASPECT_RATIO;
        }
    }
}
