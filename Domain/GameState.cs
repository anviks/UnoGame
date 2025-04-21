using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Domain.Cards;
using Domain.Players;

namespace Domain;

public class GameState
{
    public int Id { get; set; }

    public List<string> History { get; set; } = new();

    public CardDeck Deck { get; set; } = new(CardDeck.DefaultCards.ToList());
    public List<Player> Players { get; set; } = new();
    public List<Card> DiscardPile { get; set; } = new();
    public ECardColor CurrentColor { get; set; }
    public ECardValue? CurrentValue { get; set; }

    [DisplayName("Created at")]
    public DateTime CreatedAt { get; set; }
    [DisplayName("Last played at")]
    public DateTime UpdatedAt { get; set; }

    [DisplayName("Game name")]
    [Required(ErrorMessage = "Game name is required.")]
    [StringLength(32, MinimumLength = 2, ErrorMessage = "Game name length must be between 2 and 32.")]
    [RegularExpression(@"^\w[\w_.+\-@ ]*\w$", ErrorMessage = "Game name must be at least 2 characters long, start and end with a letter, digit or an underscore and can contain symbols .+-@.")]
    public string GameName { get; set; } = default!;
    public int CurrentPlayerIndex { get; set; }
    public bool IsReversed { get; set; }
}