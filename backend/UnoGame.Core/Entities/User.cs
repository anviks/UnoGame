using System.ComponentModel.DataAnnotations;

namespace UnoGame.Core.Entities;

public class User
{
    public int Id { get; set; }
    [MaxLength(64)] public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}