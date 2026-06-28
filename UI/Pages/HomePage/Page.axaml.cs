using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Logic;
using Models.Data;
using UI.Helpers;
using UI.Pages.HomePage;

namespace UI.Pages.HomePage;

public partial class Page : UserControl
{
    private int? currentSelectedCard;
    private ReusableList<ImageCard> cards;

    private ProjectCard[]? cardInfo;

    public Page()
    {
        InitializeComponent();

        cards = new ReusableList<ImageCard>(grid_Content);
    }

    public async Task Draw()
    {
        await DrawCards();
    }

    private async Task DrawCards()
    {
        cardInfo = await ProjectLogic.GetProjects();
        cards.Draw(cardInfo, (c, i, dat) => _ = c.Draw(dat, i, SelectCard));
    }

    private async Task SelectCard(int cardPos)
    {
        if (currentSelectedCard.HasValue)
            cards[currentSelectedCard.Value].ToggleSelection(false);

        currentSelectedCard = cardPos;
        cards[currentSelectedCard.Value].ToggleSelection(false);

        await control_MoreInfo.Show(cardInfo![cardPos].id);
    }
}