using Microsoft.EntityFrameworkCore;
using UnoGame.Core.Entities;

namespace DAL.Context;

public class UnoDbContext(DbContextOptions<UnoDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games { get; set; } = default!;
    public DbSet<Player> Players { get; set; } = default!;
    public DbSet<Card> Cards { get; set; } = default!;
    public DbSet<PlayerCard> PlayerCards { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<MagicToken> MagicTokens { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>()
            .HasMany(p => p.PlayerCards)
            .WithOne(pc => pc.Player)
            .HasForeignKey(pc => pc.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}