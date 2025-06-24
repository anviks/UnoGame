using Microsoft.EntityFrameworkCore;
using UnoGame.Core.Entities;

namespace DAL.Context;

public class UnoDbContext(DbContextOptions<UnoDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<MagicToken> MagicTokens { get; set; } = default!;
}