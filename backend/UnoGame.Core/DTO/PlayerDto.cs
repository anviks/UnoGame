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
}