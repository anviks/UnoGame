using UnoGame.Core.State.Enums;

namespace UnoGame.Core.State;

public class Player
{
    public string Name { get; set; } = default!;
    public PlayerType Type { get; init; }
    public bool SaidUno { get; set; }
    public int? UserId { get; set; }
    public List<Card> Cards { get; set; } = [];
    public Card? PendingDrawnCard { get; set; }

    public bool HasCard(Card card)
    {
        return Cards.Contains(card);
    }
}