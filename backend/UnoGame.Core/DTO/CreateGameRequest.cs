using UnoGame.Core.State;

namespace UnoGame.Core.DTO;

public class CreateGameRequest
{
    public string GameName { get; set; } = default!;
    public List<CreateGamePlayer> Players { get; set; } = default!;
    public List<Card> IncludedCards { get; set; } = default!;
}