using System.ComponentModel.DataAnnotations.Schema;
using UnoGame.Core.Entities.Enums;

namespace UnoGame.Core.State;

public class GameState
{
    public CardColor CurrentColor { get; set; }
    public CardValue? CurrentValue { get; set; }
    public List<string> History { get; set; } = [];
    public int CurrentPlayerIndex { get; set; }
    public bool IsReversed { get; set; }

    public List<Player> Players { get; set; } = default!;
    public List<Card> DrawPile { get; set; } = [];
    public List<Card> DiscardPile { get; set; } = [];

    // Other properties
    [NotMapped] public const int InitialHandSize = 7;
    [NotMapped] private static readonly Random Rng = new();

    public Player CurrentPlayer => Players[CurrentPlayerIndex];

    public void EndTurn()
    {
        CurrentPlayerIndex =
            (CurrentPlayerIndex + (IsReversed ? -1 : 1) + Players.Count)
            % Players.Count;

        Players.ForEach(p => p.PendingDrawnCard = null);
    }

    /**
     * Shuffles one of the piles using the Fisher-Yates algorithm.
     */
    public void ShuffleDrawPile()
    {
        var n = DrawPile.Count;
        while (n > 1)
        {
            n--;
            var k = Rng.Next(n + 1);
            (DrawPile[k], DrawPile[n]) = (DrawPile[n], DrawPile[k]);
        }
    }

    public Card? DrawCardForPlayer(Player player)
    {
        if (DrawPile.Count == 0)
        {
            ResetDrawPile();
        }

        var card = DrawCard();

        if (card != null) player.Cards.Add(card);

        return card;
    }

    private void ResetDrawPile()
    {
        Card topCard = DiscardPile[0];
        DiscardPile.RemoveAt(0);
        DrawPile.AddRange(DiscardPile);
        DiscardPile = [topCard];
        ShuffleDrawPile();
    }

    public Card? DrawCard()
    {
        if (DrawPile.Count == 0) return null;

        var pileCard = DrawPile[0];
        DrawPile.RemoveAt(0);

        return pileCard;
    }

    public Card?[] DrawCardsForPlayer(Player player, int count)
    {
        var cards = new Card?[count];

        for (int i = 0; i < count; i++)
        {
            cards[i] = DrawCardForPlayer(player);
        }

        return cards;
    }

    public void DealCards()
    {
        for (int i = 0; i < InitialHandSize; i++)
        {
            foreach (var player in Players)
            {
                DrawCardForPlayer(player);
            }
        }
    }

    public bool CanPlayerPlayCard(Player player, Card card)
    {
        return player.HasCard(card) && IsCardPlayable(player, card);
    }

    public bool IsCardPlayable(Player player, Card card)
    {
        if (card.Value == CardValue.WildDrawFour)
        {
            // Wild draw four can only be played if the player has no other cards of the current color
            return player
                .Cards
                .All(c => c.Color != CurrentColor);
        }

        return card.Color == CurrentColor
               || card.Value == CurrentValue
               || card.Value == CardValue.Wild;
    }

    public static CardColor GetColorFromBot(Player player)
    {
        var chosenColor = player.Cards.Select(pc => pc.Color)
            .GroupBy(color => color)
            .MaxBy(group => group.Count())
            ?.Key ?? CardColor.Red;

        return chosenColor;
    }

    public Card? GetCardFromBot(Player player, out bool drewCard)
    {
        var cardToPlay = player.Cards.FirstOrDefault(card => CanPlayerPlayCard(player, card));

        if (cardToPlay is null)
        {
            cardToPlay = DrawCard();
            drewCard = true;
            if (cardToPlay == null) return null;
            player.Cards.Add(cardToPlay);
            if (!CanPlayerPlayCard(player, cardToPlay)) return null;
        }
        else
        {
            drewCard = false;
        }

        player.Cards.Remove(cardToPlay);
        if (player.Cards.Count != 1) return cardToPlay;
        player.SaidUno = true;

        return cardToPlay;
    }
}