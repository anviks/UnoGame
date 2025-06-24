using UnoGame.Core.State;

namespace UnoGame.Core;

public class GameConfiguration
{
    public HashSet<Card> DisabledCards { get; set; } = new();
}