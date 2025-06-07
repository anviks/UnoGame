using System.ComponentModel.DataAnnotations;

namespace UnoGame.Core.Entities;

public class User
{
    public int Id { get; set; }
    [MaxLength(256)] public string Email { get; set; } = default!;
    public Guid Token { get; set; } = Guid.NewGuid();
    [MaxLength(64)] public string Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Player> Players { get; set; } = default!;
}