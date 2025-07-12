using UnoGame.Core.State.Enums;

namespace UnoGame.Core.DTO;

public class CreateGamePlayer
{
    public string Name { get; set; } = default!;
    public PlayerType Type { get; set; }
}