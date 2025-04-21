using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain;

public class GameStateEntity
{
    [Key] public int Id { get; set; }

    [DisplayName("Game name")]
    public string GameName { get; set; } = default!;
    [DisplayName("Created at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [DisplayName("Last played at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public string State { get; set; } = default!;
}