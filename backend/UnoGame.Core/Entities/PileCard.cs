using UnoGame.Core.Entities.Enums;

namespace UnoGame.Core.Entities;

public class PileCard
{
    public int Id { get; set; }
    public int Position { get; set; }
    public PileType PileType { get; set; }

    // Navigation properties
    public int CardId { get; set; }
    public Card Card { get; set; } = default!;

    public int GameId { get; set; }
    public Game Game { get; set; } = default!;
}