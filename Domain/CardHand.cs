using System.Text;
using Domain.Cards;

namespace Domain;

public class CardHand
{
    public List<Card> Cards { get; set; } = new();

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