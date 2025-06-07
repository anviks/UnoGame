using UnoGame.Core.Entities.Enums;

namespace WebApp.DTO;

public class CardDto
{
    public int Id { get; set; }
    public CardColor Color { get; set; }
    public CardValue Value { get; set; }
}