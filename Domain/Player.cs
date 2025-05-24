using DAL.Entities;
using DAL.Entities.Players;

namespace Domain;

public class Player
{
    public int Id { get; set; }

    public int? UserId { get; set; }
    public UserEntity? User { get; set; } = default!;

    public string Name { get; set; } = default!;
    public CardHand Hand { get; set; } = default!;
    public PlayerType Type { get; init; }
    public bool SaidUno { get; set; }
}