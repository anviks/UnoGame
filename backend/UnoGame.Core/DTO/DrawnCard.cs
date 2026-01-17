using UnoGame.Core.State;

namespace UnoGame.Core.DTO;

public class DrawnCard
{
    public int Index { get; set; }
    public Card Card { get; set; } = null!;
}