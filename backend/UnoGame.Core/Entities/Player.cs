using System.ComponentModel.DataAnnotations;
using UnoGame.Core.Entities.Enums;

namespace UnoGame.Core.Entities;

public class Player
{
    public int Id { get; set; }
    [MaxLength(64)]
    public string Name { get; set; } = default!;
    public PlayerType Type { get; init; }
    public bool SaidUno { get; set; }
    public int Position { get; set; }

    // Navigation properties
    public int? UserId { get; set; }
    public User? User { get; set; }

    public int GameId { get; set; }
    public Game Game { get; set; } = default!;

    public ICollection<PlayerCard> PlayerCards { get; set; } = default!;

    public PlayerCard? FindPlayerCard(CardColor color, CardValue value)
    {
        return PlayerCards.FirstOrDefault(pc => pc.Card.Color == color && pc.Card.Value == value);
    }

    public Card? RemoveCard(CardColor color, CardValue value)
    {
        PlayerCard? removedCard = FindPlayerCard(color, value);
        if (removedCard == null) return null;
        PlayerCards.Remove(removedCard);

        return removedCard.Card;
    }
}