using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAL.Entities.Players;

namespace DAL.Entities.Cards;

public class CardEntity
{
    [Key] public int Id { get; set; }

    public CardColor Color { get; init; }
    public CardValue Value { get; init; }
    public CardLocation Location { get; set; }
    public int Position { get; set; }

    // If the card is in a player's hand, this will be the player id
    public int? PlayerId { get; set; }
    public PlayerEntity? Player { get; set; }

    public int GameId { get; set; }
    public GameEntity GameEntity { get; set; } = default!;
}