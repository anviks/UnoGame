using DAL.Entities;
using DAL.Entities.Cards;
using DAL.Entities.Players;
using Microsoft.EntityFrameworkCore;

namespace DAL.Context;

public class UnoDbContext(DbContextOptions<UnoDbContext> options) : DbContext(options)
{
    public DbSet<GameEntity> Games { get; set; } = default!;
    public DbSet<PlayerEntity> Players { get; set; } = default!;
    public DbSet<CardEntity> Cards { get; set; } = default!;
    public DbSet<UserEntity> Users { get; set; } = default!;
    public DbSet<MagicToken> MagicTokens { get; set; } = default!;
}