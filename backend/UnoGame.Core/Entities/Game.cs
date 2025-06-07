using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UnoGame.Core.Entities.Enums;

namespace UnoGame.Core.Entities;

public class Game
{
    public int Id { get; set; }
    [MaxLength(64)] public string Name { get; set; } = default!;
    public CardColor CurrentColor { get; set; }
    public CardValue? CurrentValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> History { get; set; } = [];
    public int CurrentPlayerIndex { get; set; }
    public bool IsReversed { get; set; }

    // Navigation properties
    public List<PileCard> PileCards { get; set; } = default!;

    public List<Player> Players { get; set; } = default!;

    // Other properties
    [NotMapped] public const int InitialHandSize = 7;
    [NotMapped] private static readonly Random Rng = new();
    [NotMapped] public List<Card> DrawPile { get; private set; } = [];
    [NotMapped] public List<Card> DiscardPile { get; private set; } = [];
    [NotMapped] private bool _pilesAreStale = true;
    [NotMapped] private bool _pileCardsAreStale = false;

    public Player CurrentPlayer => Players[CurrentPlayerIndex];

    public void EndTurn() =>
        CurrentPlayerIndex =
            (CurrentPlayerIndex + (IsReversed ? -1 : 1) + Players.Count)
            % Players.Count;

    public void SyncPilesFromPileCards()
    {
        if (_pileCardsAreStale)
            throw new InvalidOperationException("PileCards are stale. Please sync PileCards first.");

        DrawPile = PileCards
            .Where(pc => pc.PileType == PileType.DrawPile)
            .OrderBy(pc => pc.Position)
            .Select(pc => pc.Card)
            .ToList();

        DiscardPile = PileCards
            .Where(pc => pc.PileType == PileType.DiscardPile)
            .OrderBy(pc => pc.Position)
            .Select(pc => pc.Card)
            .ToList();

        _pileCardsAreStale = true;
        _pilesAreStale = false;
    }

    public void SyncPileCardsFromPiles()
    {
        if (_pilesAreStale)
            throw new InvalidOperationException("Piles are stale. Please sync Piles first.");

        PileCards = [];

        for (int i = 0; i < DrawPile.Count; i++)
        {
            // DrawPile[i].PileType = PileType.DrawPile;
            // DrawPile[i].Position = i;
            PileCards.Add(new PileCard { Card = DrawPile[i], PileType = PileType.DrawPile, Position = i });
        }

        for (int i = 0; i < DiscardPile.Count; i++)
        {
            // DiscardPile[i].PileType = PileType.DiscardPile;
            // DiscardPile[i].Position = i;
            PileCards.Add(new PileCard { Card = DiscardPile[i], PileType = PileType.DiscardPile, Position = i });
        }

        // PileCards = DrawPile.Concat(DiscardPile).ToList();

        _pilesAreStale = true;
        _pileCardsAreStale = false;
    }

    public void EnsurePilesSynced()
    {
        if (_pilesAreStale)
            SyncPilesFromPileCards();
    }

    public void EnsurePileCardsSynced()
    {
        if (_pileCardsAreStale)
            SyncPileCardsFromPiles();
    }

    /**
     * Shuffles one of the piles using the Fisher-Yates algorithm.
     */
    public void ShuffleDrawPile()
    {
        EnsurePilesSynced();

        var n = DrawPile.Count;
        while (n > 1)
        {
            n--;
            var k = Rng.Next(n + 1);
            (DrawPile[k], DrawPile[n]) = (DrawPile[n], DrawPile[k]);
        }
    }

    public Card? GiveCard(Player player)
    {
        EnsurePilesSynced();

        if (DrawPile.Count == 0)
        {
            ResetDrawPile();
        }

        var card = DrawCard();

        if (card != null) player.PlayerCards.Add(new PlayerCard { Card = card, Player = player });

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

    public Card?[] GiveCards(Player player, int count)
    {
        var cards = new Card?[count];

        for (int i = 0; i < count; i++)
        {
            cards[i] = GiveCard(player);
        }

        return cards;
    }

    public void DealCards()
    {
        EnsurePilesSynced();

        for (int i = 0; i < InitialHandSize; i++)
        {
            foreach (var player in Players)
            {
                GiveCards(player, 1);
            }
        }
    }

    public bool HasCard(Player player, Card card)
    {
        return player.PlayerCards.Any(pc => pc.Card == card);
    }

    public bool CanPlayCard(Player player, Card card)
    {
        if (card.Value == CardValue.WildDrawFour)
        {
            // Wild draw four can only be played if the player has no other cards of the current color
            return player
                .PlayerCards
                .All(c => c.Card.Color != CurrentColor);
        }

        return card.Color == CurrentColor
               || card.Value == CurrentValue
               || card.Value == CardValue.Wild;
    }

    public static CardColor GetColorFromBot(Player player)
    {
        var chosenColor = player.PlayerCards.Select(pc => pc.Card.Color)
            .GroupBy(color => color)
            .MaxBy(group => group.Count())
            ?.Key ?? CardColor.Red;

        return chosenColor;
    }

    public Card? GetCardFromBot(Player player, out bool drewCard)
    {
        var cardToPlay = player.PlayerCards.FirstOrDefault(pc => CanPlayCard(player, pc.Card))?.Card;

        if (cardToPlay is null)
        {
            cardToPlay = DrawCard();
            drewCard = true;
            if (cardToPlay == null) return null;
            player.PlayerCards.Add(new PlayerCard { Card = cardToPlay, Player = player });
            if (!CanPlayCard(player, cardToPlay)) return null;
        }
        else
        {
            drewCard = false;
        }

        player.PlayerCards.Remove(player.PlayerCards.First(pc => pc.Card == cardToPlay));
        if (player.PlayerCards.Count != 1) return cardToPlay;
        player.SaidUno = true;

        return cardToPlay;
    }
}