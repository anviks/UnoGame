using UnoGame.Core.State;

namespace WebApp.DTO;

public class GameDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Player> Players { get; set; } = default!;
}