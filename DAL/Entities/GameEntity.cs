using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAL.Entities.Cards;
using DAL.Entities.Players;

namespace DAL.Entities;

public class GameEntity
{
    [Key] public int Id { get; set; }

    public List<string> History { get; set; } = default!;

    public List<CardEntity> Cards { get; set; } = default!;
    public List<PlayerEntity> Players { get; set; } = default!;
    public CardColor CurrentColor { get; set; }
    public CardValue? CurrentValue { get; set; }

    [DisplayName("Created at")] public DateTime CreatedAt { get; set; }
    [DisplayName("Last played at")] public DateTime UpdatedAt { get; set; }

    [DisplayName("Game name")]
    [Required(ErrorMessage = "Game name is required.")]
    [StringLength(32, MinimumLength = 2, ErrorMessage = "Game name length must be between 2 and 32.")]
    [RegularExpression(@"^\w[\w_.+\-@ ]*\w$",
        ErrorMessage =
            "Game name must be at least 2 characters long, start and end with a letter, digit or an underscore and can contain symbols .+-@.")]
    public string GameName { get; set; } = default!;

    public int CurrentPlayerIndex { get; set; }
    public bool IsReversed { get; set; }
}