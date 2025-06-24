using System.Text;
using UnoGame.Core.State;

namespace UnoGame.Core;

public class CardHand(List<Card> cards)
{
    public List<Card> Cards { get; set; } = cards;

    public Card this[int index] => Cards[index];

    public bool Remove(Card card) => Cards.Remove(card);

    public void Sort() =>
        Cards.Sort((a, b) =>
        {
            var colorComparison = a.Color.CompareTo(b.Color);
            return colorComparison != 0 ? colorComparison : a.Value.CompareTo(b.Value);
        });

    public void Add(params Card[] cards)
    {
        Cards.AddRange(cards);
        Sort();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < Cards.Count; i++)
        {
            sb.Append($"{i + 1}: {Cards[i]}");
            if (i < Cards.Count - 1)
            {
                sb.Append(", ");
            }
        }

        return sb.ToString();
    }
}