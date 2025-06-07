namespace UnoGame.Core.Entities;

public class PlayerCard
{
    public int Id { get; set; }

    // Navigation properties
    public int PlayerId { get; set; }
    public Player Player { get; set; } = default!;

    public int CardId { get; set; }
    public Card Card { get; set; } = default!;
}