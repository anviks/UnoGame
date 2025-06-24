using System.ComponentModel.DataAnnotations;

namespace UnoGame.Core.Entities;

public class Game
{
    public int Id { get; set; }
    [MaxLength(64)] public string Name { get; set; } = default!;
    public string SerializedState { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}