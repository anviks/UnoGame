using UnoGame.Core.State;
using UnoGame.Core.State.Enums;

namespace UnoGame.Core.DTO;

public class GameStateDto
{
    public CardColor? CurrentColor { get; set; }
    public CardValue? CurrentValue { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public bool IsReversed { get; set; }
    public int? WinnerIndex { get; set; }
    public PendingPenalty? PendingPenalty { get; set; }

    public List<PlayerDto> Players { get; set; } = default!;

    public int DrawPileSize { get; set; }
    public List<Card> DiscardPile { get; set; } = [];
}
