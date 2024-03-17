using System.ComponentModel.DataAnnotations;

namespace Domain.Players;

public class Player
{
    [StringLength(32, MinimumLength = 2, ErrorMessage = "Player name length must be between 2 and 32.")]
    [RegularExpression(@"^\w[\w_.+\-@ ]*\w$", ErrorMessage = "Player name must be at least 2 characters long, start and end with a letter, digit or an underscore and can contain symbols .+-@.")]
    [Required(ErrorMessage = "Player name is required.")]
    public string Name { get; set; } = default!;
    public CardHand Hand { get; set; } = new();
    public EPlayerType Type { get; init; }
    public bool SaidUno { get; set; }

    public Player(string name, EPlayerType type)
    {
        Name = name;
        Type = type;
    }

    public Player()
    {
    }

    public override string ToString() => Name;
}