using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Models.Data;
using UI.Helpers;

namespace UI.Pages.HomePage.ContentDisplays;

public interface IHomePageLayout
{
    public void ToggleVisibility(bool to);
    public Task Draw(ProjectInfo[] cards, Func<int, Task> onSelect);
    public void UpdateSelection(int to);
}

public abstract class HomePageLayoutBase<T> : IHomePageLayout where T : UserControl
{
    protected Panel container;
    protected ReusableList<T> cards;

    public HomePageLayoutBase(Panel scroller)
    {
        container = GetWrapper();

        scroller.Children.Add(container);
        cards = new ReusableList<T>(container);
    }


    public abstract Task Draw(ProjectInfo[] cardInfo, Func<int, Task> onSelect);

    public void ToggleVisibility(bool to)
    {
        container.IsVisible = to;
    }

    public void UpdateSelection(int to)
    {
        for (int i = 0; i < cards.getElementCount; i++)
            ToggleElementSelection(cards[i], i == to);
    }

    protected abstract Panel GetWrapper();
    protected abstract void ToggleElementSelection(T element, bool to);
}