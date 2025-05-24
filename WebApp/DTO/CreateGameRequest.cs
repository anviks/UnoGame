using DAL.Entities.Players;
using Domain;

namespace WebApp.DTO;

public class CreateGameRequest
{
    public string GameName { get; set; } = default!;
    public List<Player> Players { get; set; } = default!;
    public List<Card> Deck { get; set; } = default!;

    public class Player
    {
        public string Name { get; set; } = default!;
        public PlayerType Type { get; set; } = default!;
    }
}