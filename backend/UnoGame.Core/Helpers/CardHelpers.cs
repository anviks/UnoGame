using UnoGame.Core.State;
using UnoGame.Core.State.Enums;

namespace UnoGame.Core.Helpers;

public static class CardHelpers
{
    public static readonly IReadOnlyCollection<Card> DefaultCards;

    static CardHelpers()
    {
        var defaultCards = new List<Card>();

        for (var i = 0; i < (int)CardColor.Wild; i++)
        {
            for (var j = 0; j < (int)CardValue.Wild; j++)
            {
                var color = (CardColor)i;
                var value = (CardValue)j;

                defaultCards.Add(new Card { Color = color, Value = value });
                if (j > 0)
                {
                    defaultCards.Add(new Card { Color = color, Value = value });
                }
            }
        }

        for (var i = 0; i < 4; i++)
        {
            defaultCards.Add(new Card { Color = CardColor.Wild, Value = CardValue.Wild });
        }

        for (var i = 0; i < 4; i++)
        {
            defaultCards.Add(new Card { Color = CardColor.Wild, Value = CardValue.WildDrawFour });
        }

        DefaultCards = defaultCards.AsReadOnly();
    }

    public static void SortCards(this List<Card> cards)
    {
        cards.Sort((a, b) =>
        {
            var colorComparison = a.Color.CompareTo(b.Color);
            return colorComparison != 0 ? colorComparison : a.Value.CompareTo(b.Value);
        });
    }
}