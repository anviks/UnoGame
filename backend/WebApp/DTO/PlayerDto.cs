using UnoGame.Core.Entities.Enums;

namespace WebApp.DTO;

public class PlayerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public PlayerType Type { get; init; }
    public bool SaidUno { get; set; }
    public int Position { get; set; }

    // Navigation properties
    public int? UserId { get; set; }

    public List<CardDto> Cards { get; set; } = default!;
}