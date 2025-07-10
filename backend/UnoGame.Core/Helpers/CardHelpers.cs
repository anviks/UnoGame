using UnoGame.Core.State;

namespace UnoGame.Core.Helpers;

public static class CardHelpers
{
    public static void SortCards(this List<Card> cards)
    {
        cards.Sort((a, b) =>
        {
            var colorComparison = a.Color.CompareTo(b.Color);
            return colorComparison != 0 ? colorComparison : a.Value.CompareTo(b.Value);
        });
    }
}