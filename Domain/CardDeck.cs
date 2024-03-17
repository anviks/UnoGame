using Domain.Cards;

namespace Domain;

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

        for (int i = 0; i < (int)ECardColor.Wild; i++)
        {
            for (int j = 0; j < (int)ECardValue.Wild; j++)
            {
                var color = (ECardColor)i;
                var value = (ECardValue)j;

                defaultCards.Add(new Card(color, value));
                if (j > 0)
                {
                    defaultCards.Add(new Card(color, value));
                }
            }
        }

        for (var i = 0; i < 4; i++)
        {
            defaultCards.Add(new Card(ECardColor.Wild, ECardValue.Wild));
        }

        for (var i = 0; i < 4; i++)
        {
            defaultCards.Add(new Card(ECardColor.Wild, ECardValue.WildDrawFour));
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

    /**
     * Shuffles the deck using the Fisher-Yates algorithm.
     */
    public void Shuffle()
    {
        var n = Cards.Count;
        while (n > 1)
        {
            n--;
            var k = Rng.Next(n + 1);
            (Cards[k], Cards[n]) = (Cards[n], Cards[k]);
        }
    }

    public void InsertRandomly(Card card)
    {
        Cards.Insert(Rng.Next(Cards.Count), card);
    }

    public Card? DrawCard()
    {
        int lastIndex = Cards.Count - 1;

        if (lastIndex < 0) return null;

        var card = Cards[lastIndex];
        Cards.RemoveAt(lastIndex);
        return card;
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