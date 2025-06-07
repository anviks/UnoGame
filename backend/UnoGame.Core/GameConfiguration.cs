using UnoGame.Core.Entities;

namespace UnoGame.Core;

public class GameConfiguration
{
    public HashSet<Card> DisabledCards { get; set; } = new();
}