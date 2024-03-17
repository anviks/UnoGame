using System.Xml;
using Domain;
using Domain.Cards;
using Domain.Players;

namespace UnoEngine;

public class Engine
{
    public GameState State { get; set; } = default!;
    public const int InitialHandSize = 7;

    public void ApplyConfiguration(GameConfiguration configuration)
    {
        ApplyConfiguration(State, configuration);
    }

    public static void ApplyConfiguration(GameState state, GameConfiguration configuration)
    {
        state.Deck.Cards.RemoveAll(card => configuration.DisabledCards.Contains(card));
        if (!IsValidConfiguration(configuration, state.Players.Count))
            state.Deck.Cards = CardDeck.DefaultCards.ToList();
    }

    public static bool IsValidConfiguration(GameConfiguration configuration, int playerCount)
    {
        // If there are not enough cards in the deck to start the game
        // (InitialHandSize cards for each player + 1 card to the discard pile),
        // the configuration is invalid
        return CardDeck.DefaultCards.Count(card => !configuration.DisabledCards.Contains(card)) >=
               playerCount * InitialHandSize + 1;
    }

    public Card? GiveCard(Player player)
    {
        var card = State.Deck.DrawCard();

        if (State.Deck.Cards.Count == 0)
        {
            var topCard = State.DiscardPile[^1];
            State.DiscardPile.RemoveAt(State.DiscardPile.Count - 1);
            State.Deck = new CardDeck(State.DiscardPile);
            State.Deck.Shuffle();
            State.DiscardPile = new List<Card> { topCard };
        }

        if (card != null) player.Hand.Add(card);

        return card;
    }

    public Card[] GiveCards(Player player, int count)
    {
        var cards = new Card[count];

        for (int i = 0; i < count; i++)
        {
            cards[i] = GiveCard(player);
        }

        return cards;
    }

    public Player GetCurrentPlayer() => State.Players[State.CurrentPlayerIndex];

    public void NextPlayer() =>
        State.CurrentPlayerIndex =
            (State.CurrentPlayerIndex + (State.IsReversed ? -1 : 1) + State.Players.Count)
            % State.Players.Count;

    public void DealCards()
    {
        for (int i = 0; i < InitialHandSize; i++)
        {
            foreach (var player in State.Players)
            {
                GiveCards(player, 1);
            }
        }
    }

    public bool CanPlayCard(Player player, int index)
        => CanPlayCard(player, player.Hand[index]);

    public bool CanPlayCard(Player player, Card card)
    {
        if (card.Value == ECardValue.WildDrawFour)
        {
            // Wild draw four can only be played if the player has no other cards of the current color
            return player.Hand
                .Cards
                .All(c => c.Color != State.CurrentColor);
        }

        return card.Color == State.CurrentColor
               || card.Value == State.CurrentValue
               || card.Value == ECardValue.Wild;
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

    public static bool IsValidColor(string? input, out ECardColor? color)
    {
        var validColors = new[] { "red", "green", "blue", "yellow" };
        color = null;
        if (input == null || !validColors.Contains(input)) return false;

        color = input switch
        {
            "red" => ECardColor.Red,
            "green" => ECardColor.Green,
            "blue" => ECardColor.Blue,
            "yellow" => ECardColor.Yellow,
            _ => throw new ArgumentOutOfRangeException(paramName: input)
        };

        return true;
    }

    public static ECardColor GetColorFromBot(Player player)
    {
        var chosenColor = player.Hand.Cards.Select(card => card.Color)
            .GroupBy(color => color)
            .MaxBy(group => group.Count())
            ?.Key ?? ECardColor.Red;

        return chosenColor;
    }

    public Card? GetCardFromBot(Player player, out bool drewCard)
    {
        var cardToPlay = player.Hand.Cards.FirstOrDefault(card => CanPlayCard(player, card));

        if (cardToPlay is null)
        {
            cardToPlay = State.Deck.DrawCard();
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