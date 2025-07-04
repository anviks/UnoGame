using UnoGame.Core.Entities.Enums;
using UnoGame.Core.State;

namespace UnoGame.Core;

public class CardDeck
{
    public List<Card> Cards { get; set; } = default!;
    private Random Rng { get; set; } = new();
    public static readonly IReadOnlyCollection<Card> DefaultCards;
    public static readonly IReadOnlyCollection<Card> DefaultCardsDistinct;
    public static readonly IReadOnlyDictionary<string, Card> StringToCard;

    static CardDeck()
    {
        var defaultCards = new List<Card>();

        for (int i = 0; i < (int)CardColor.Wild; i++)
        {
            for (int j = 0; j < (int)CardValue.Wild; j++)
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
        DefaultCardsDistinct = defaultCards.Distinct().ToList().AsReadOnly();
        StringToCard = DefaultCardsDistinct.ToDictionary(card => card.ToString());
    }

    public CardDeck()
    {
    }

    public CardDeck(List<Card> cards)
    {
        Cards = cards;
    }

    public void InsertRandomly(Card card)
    {
        Cards.Insert(Rng.Next(Cards.Count), card);
    }

    public Card GetLast()
    {
        return Cards.Last();
    }

    public int Size()
    {
        return Cards.Count;
    }

    public Dictionary<Card, int> GetCardCounts()
    {
        var cardCounts = new Dictionary<Card, int>();

        foreach (var card in Cards)
        {
            if (cardCounts.TryGetValue(card, out var value))
            {
                cardCounts[card] = value + 1;
            }
            else
            {
                cardCounts.Add(card, 1);
            }
        }

        return cardCounts;
    }

    public override string ToString()
    {
        return string.Join("\n", Cards);
    }
}