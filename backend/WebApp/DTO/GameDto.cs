using UnoGame.Core.Entities.Enums;

namespace WebApp.DTO;

public class GameDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public CardColor CurrentColor { get; set; }
    public CardValue? CurrentValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> History { get; set; } = [];
    public int CurrentPlayerIndex { get; set; }
    public bool IsReversed { get; set; }

    public List<PlayerDto> Players { get; set; } = default!;
    public List<CardDto> DrawPile { get; set; } = [];
    public List<CardDto> DiscardPile { get; set; } = [];
}