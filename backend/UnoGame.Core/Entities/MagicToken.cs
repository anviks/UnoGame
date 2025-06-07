using System.ComponentModel.DataAnnotations;

namespace UnoGame.Core.Entities;

public class MagicToken
{
    [Key] public Guid Token { get; set; } = Guid.NewGuid();
    [MaxLength(256)] public string Email { get; set; } = default!;
    public DateTime Expiry { get; set; } = DateTime.UtcNow.AddMinutes(15);
    public bool Used { get; set; } = false;
}