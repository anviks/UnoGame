using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DAL.Entities;
using DAL.Entities.Cards;

namespace Domain;

public class Game
{
    public int Id { get; set; }

    public List<string> History { get; set; } = [];

    public CardDeck Deck { get; set; } = default!;
    public List<Card> DiscardPile { get; set; } = [];

    public List<Player> Players { get; set; } = [];
    public CardColor CurrentColor { get; set; }
    public CardValue? CurrentValue { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string GameName { get; set; } = default!;

    public int CurrentPlayerIndex { get; set; }
    public bool IsReversed { get; set; }

    public const int InitialHandSize = 7;

    public Card? GiveCard(Player player)
    {
        var card = this.Deck.DrawCard();

        if (this.Deck.Cards.Count == 0)
        {
            var topCard = this.DiscardPile[^1];
            this.DiscardPile.RemoveAt(this.DiscardPile.Count - 1);
            this.Deck = new CardDeck(this.DiscardPile);
            this.Deck.Shuffle();
            this.DiscardPile = new List<Card> { topCard };
        }

        if (card != null) player.Hand.Add(card);

        return card;
    }

    public Card?[] GiveCards(Player player, int count)
    {
        var cards = new Card?[count];

        for (int i = 0; i < count; i++)
        {
            cards[i] = GiveCard(player);
        }

        return cards;
    }

    public Player GetCurrentPlayer() => this.Players[this.CurrentPlayerIndex];

    public void NextPlayer() =>
        this.CurrentPlayerIndex =
            (this.CurrentPlayerIndex + (this.IsReversed ? -1 : 1) + this.Players.Count)
            % this.Players.Count;

    public void DealCards()
    {
        for (int i = 0; i < InitialHandSize; i++)
        {
            foreach (var player in this.Players)
            {
                GiveCards(player, 1);
            }
        }
    }

    public bool CanPlayCard(Player player, int index)
        => CanPlayCard(player, player.Hand[index]);

    public bool CanPlayCard(Player player, Card card)
    {
        if (card.Value == CardValue.WildDrawFour)
        {
            // Wild draw four can only be played if the player has no other cards of the current color
            return player.Hand
                .Cards
                .All(c => c.Color != this.CurrentColor);
        }

        return card.Color == this.CurrentColor
               || card.Value == this.CurrentValue
               || card.Value == CardValue.Wild;
    }

    /// <summary>
    /// Validates user's card choice.
    /// </summary>
    /// <param name="player">The user's Player object</param>
    /// <param name="input">User's choice for card to play</param>
    /// <param name="base">The base index of the player's hand</param>
    /// <returns>Zero-based index of the card to play, or -1 if the input is invalid.</returns>
    public static int ValidateCardIndex(Player player, string? input, int @base)
    {
        if (input is not null
            && int.TryParse(input, out var cardIndex)
            && cardIndex >= @base
            && cardIndex < @base + player.Hand.Cards.Count)
        {
            return cardIndex - @base;
        }

        return -1;
    }

    public static bool IsValidColor(string? input, out CardColor? color)
    {
        var validColors = new[] { "red", "green", "blue", "yellow" };
        color = null;
        if (input == null || !validColors.Contains(input)) return false;

        color = input switch
        {
            "red" => CardColor.Red,
            "green" => CardColor.Green,
            "blue" => CardColor.Blue,
            "yellow" => CardColor.Yellow,
            _ => throw new ArgumentOutOfRangeException(paramName: input)
        };

        return true;
    }

    public static CardColor GetColorFromBot(Player player)
    {
        var chosenColor = player.Hand.Cards.Select(card => card.Color)
            .GroupBy(color => color)
            .MaxBy(group => group.Count())
            ?.Key ?? CardColor.Red;

        return chosenColor;
    }

    public Card? GetCardFromBot(Player player, out bool drewCard)
    {
        var cardToPlay = player.Hand.Cards.FirstOrDefault(card => CanPlayCard(player, card));

        if (cardToPlay is null)
        {
            cardToPlay = this.Deck.DrawCard();
            drewCard = true;
            // if (!PlayerDrewCard(player, cardToPlay)) return null;
            if (cardToPlay == null) return null;
            player.Hand.Add(cardToPlay);
            // SlowWriteLine(CanRevealCards(player)
            //     ? $"{player.Name} drew {cardToPlay}"
            //     : $"{player.Name} drew a card");
            if (!CanPlayCard(player, cardToPlay)) return null;
        }
        else
        {
            drewCard = false;
        }

        player.Hand.Remove(cardToPlay);
        if (player.Hand.Cards.Count != 1) return cardToPlay;
        // RegisterSaidUno(player);
        player.SaidUno = true;

        return cardToPlay;
    }
}