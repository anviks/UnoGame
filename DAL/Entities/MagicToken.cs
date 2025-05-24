using System.ComponentModel.DataAnnotations;

namespace DAL.Entities;

public class MagicToken
{
    [Key] public Guid Token { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = default!;
    public DateTime Expiry { get; set; } = DateTime.UtcNow.AddMinutes(15);
    public bool Used { get; set; } = false;
}