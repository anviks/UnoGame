using static Helpers.AnsiConstants;

namespace Domain.Cards;

public class Card
{
    public ECardColor Color { get; init; }
    public ECardValue Value { get; init; }

    public Card(ECardColor color, ECardValue value)
    {
        Color = color;
        Value = value;
    }

    public Card()
    {
    }

    public override bool Equals(object? obj)
    {
        return obj is Card card &&
               Color == card.Color &&
               Value == card.Value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Color, (int)Value);
    }

    public override string ToString()
    {
        var cardString = "[";

        cardString += Color switch
        {
            ECardColor.Red => RedBright,
            ECardColor.Yellow => Yellow,
            ECardColor.Green => GreenBright,
            ECardColor.Blue => BlueBright,
            _ => ""
        };

        cardString += Value switch
        {
            ECardValue.Zero => "0",
            ECardValue.One => "1",
            ECardValue.Two => "2",
            ECardValue.Three => "3",
            ECardValue.Four => "4",
            ECardValue.Five => "5",
            ECardValue.Six => "6",
            ECardValue.Seven => "7",
            ECardValue.Eight => "8",
            ECardValue.Nine => "9",
            ECardValue.DrawTwo => "DRAW 2",
            ECardValue.Reverse => "REVERSE",
            ECardValue.Skip => "SKIP",
            ECardValue.Wild => $"{RedBright}W{Yellow}I{GreenBright}L{BlueBright}D",
            ECardValue.WildDrawFour => $"{RedBright}D{Yellow}R{GreenBright}A{BlueBright}W{RedBright} 4",
            _ => throw new ArgumentOutOfRangeException()
        };

        return cardString + Reset + "]";
    }
}