using UnoGame.Core.Entities.Enums;

namespace UnoGame.Core.Entities;

public class Card
{
    public int Id { get; set; }
    public CardColor Color { get; init; }
    public CardValue Value { get; init; }

    // Navigation properties
    public ICollection<PlayerCard> PlayerCards { get; set; } = default!;

    public ICollection<PileCard> PileCards { get; set; } = default!;

    public override bool Equals(object? obj)
    {
        return obj is Card card
               && Id == card.Id
               && Color == card.Color
               && Value == card.Value;
    }

    public static bool operator ==(Card? a, Card? b)
    {
        if (ReferenceEquals(a, b))
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(Card? a, Card? b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Color, (int)Value);
    }

    public override string ToString()
    {
        return $"{Color} {Value}";
    }
}