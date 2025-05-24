using System.ComponentModel.DataAnnotations;
using DAL.Entities.Players;
using Microsoft.EntityFrameworkCore;

namespace DAL.Entities;

[Index(nameof(Email), IsUnique = true)]
public class UserEntity
{
    [Key] public int Id { get; set; }
    [MaxLength(64)]
    public string Email { get; set; } = default!;
    public Guid Token { get; set; } = Guid.NewGuid();
    [MaxLength(64)]
    public string Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<PlayerEntity> Players { get; set; } = default!;
}