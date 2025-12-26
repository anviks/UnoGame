using UnoGame.Core.State.Enums;

namespace UnoGame.Core.DTO;

public class CreateGamePlayer
{
    public string Username { get; set; } = default!;
    public PlayerType Type { get; set; }
}