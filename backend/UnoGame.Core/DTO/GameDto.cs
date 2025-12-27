namespace UnoGame.Core.DTO;

public class GameDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public GameStateDto State { get; set; } = default!;
}