using UnoGame.Core.State.Enums;

namespace UnoGame.Core.State;

public class Card
{
    public int Id { get; init; }
    public CardColor Color { get; init; }
    public CardValue Value { get; init; }

    public override bool Equals(object? obj) =>
        obj is Card card
        && Id == card.Id
        && Color == card.Color
        && Value == card.Value;

    public bool IsSimilar(Card other) =>
        Color == other.Color
        && Value == other.Value;

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

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString()
    {
        return $"{Color} {Value}";
    }
}