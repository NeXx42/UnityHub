using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Models.Data;
using UI.Controls;
using UI.Helpers;

namespace UI.Pages.HomePage.ContentDisplays;

public interface IHomePageLayout
{
    public ButtonWrapper CreateButton();

    public int getTake { get; }

    public void ToggleVisibility(bool to);
    public Task Draw(ProjectInfo[] cards, bool isPageIncrement, int currentPage, int resultCount, Func<int, Task> onSelect);
    public void UpdateSelection(int to);
}

public abstract class HomePageLayoutBase<T> : IHomePageLayout where T : UserControl
{
    public virtual int getTake => 16;

    protected ButtonWrapper? selectionButton;

    protected Panel container;
    protected ReusableList<T> cards;

    protected Action<int> onPageChangeCallback;

    public HomePageLayoutBase(Panel scroller, Action<int> onPageChange)
    {
        container = GetWrapper();

        scroller.Children.Add(container);
        cards = new ReusableList<T>(container);

        onPageChangeCallback = onPageChange;
    }


    public abstract Task Draw(ProjectInfo[] cardInfo, bool isPageIncrement, int currentPage, int resultCount, Func<int, Task> onSelect);

    public virtual void ToggleVisibility(bool to)
    {
        container.IsVisible = to;

        if (to)
        {
            selectionButton!.Classes.Remove("Transparent");
            selectionButton!.Classes.Add("Primary");
        }
        else
        {
            selectionButton!.Classes.Add("Transparent");
            selectionButton!.Classes.Remove("Primary");

            cards.Clear();
        }
    }

    public virtual void UpdateSelection(int to)
    {
        for (int i = 0; i < cards.getElementCount; i++)
            ToggleElementSelection(cards[i], i == to);
    }

    public virtual ButtonWrapper CreateButton()
    {
        selectionButton = new ButtonWrapper();
        selectionButton.Label = "Layout";

        return selectionButton;
    }

    protected abstract Panel GetWrapper();
    protected abstract void ToggleElementSelection(T element, bool to);
}