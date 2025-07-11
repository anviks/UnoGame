using UnoGame.Core.State;
using UnoGame.Core.State.Enums;

namespace UnoGame.Core.DTO;

public class PlayerDto
{
    public string Name { get; set; } = default!;
    public PlayerType Type { get; init; }
    public bool SaidUno { get; set; }
    public int? UserId { get; set; }
    public List<Card>? Cards { get; set; }
    public int HandSize { get; set; }
    public Card? PendingDrawnCard { get; set; }

    public static PlayerDto FromPlayer(Player player, bool isSelf)
    {
        return new PlayerDto
        {
            Name = player.Name,
            Type = player.Type,
            SaidUno = player.SaidUno,
            UserId = player.UserId,
            Cards = isSelf ? player.Cards : null,
            HandSize = player.Cards.Count,
            PendingDrawnCard = player.PendingDrawnCard,
        };
    }
}