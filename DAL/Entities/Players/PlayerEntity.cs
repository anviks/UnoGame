using System.ComponentModel.DataAnnotations;
using DAL.Entities.Cards;

namespace DAL.Entities.Players;

public class PlayerEntity
{
    [Key] public int Id { get; set; }

    public int? UserId { get; set; }
    public UserEntity? User { get; set; } = default!;

    public int GameId { get; set; }
    public GameEntity Game { get; set; } = default!;

    [MaxLength(64)]
    public string Name { get; set; } = default!;
    public List<CardEntity> Hand { get; set; } = [];
    public PlayerType Type { get; init; }
    public bool SaidUno { get; set; }
}