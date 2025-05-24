using DAL.Entities.Cards;

namespace Domain;

public class GameConfiguration
{
    public HashSet<Card> DisabledCards { get; set; } = new();
}