using UnoGame.Core.State;

namespace WebApp.DTO;

public class CreateGameRequest
{
    public string GameName { get; set; } = default!;
    public List<Player> Players { get; set; } = default!;
    public List<Card> IncludedCards { get; set; } = default!;
}