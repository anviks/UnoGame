using DAL.Entities.Cards;
using static Helpers.AnsiConstants;

namespace Domain;

public class Card
{
    public int Id { get; set; }

    public CardColor Color { get; init; }
    public CardValue Value { get; init; }

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
            CardColor.Red => RedBright,
            CardColor.Yellow => Yellow,
            CardColor.Green => GreenBright,
            CardColor.Blue => BlueBright,
            _ => ""
        };

        cardString += Value switch
        {
            CardValue.Zero => "0",
            CardValue.One => "1",
            CardValue.Two => "2",
            CardValue.Three => "3",
            CardValue.Four => "4",
            CardValue.Five => "5",
            CardValue.Six => "6",
            CardValue.Seven => "7",
            CardValue.Eight => "8",
            CardValue.Nine => "9",
            CardValue.DrawTwo => "DRAW 2",
            CardValue.Reverse => "REVERSE",
            CardValue.Skip => "SKIP",
            CardValue.Wild => $"{RedBright}W{Yellow}I{GreenBright}L{BlueBright}D",
            CardValue.WildDrawFour => $"{RedBright}D{Yellow}R{GreenBright}A{BlueBright}W{RedBright} 4",
            _ => throw new ArgumentOutOfRangeException()
        };

        return cardString + Reset + "]";
    }
}