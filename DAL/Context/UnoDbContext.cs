using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Context;

public class UnoDbContext(DbContextOptions<UnoDbContext> options) : DbContext(options)
{
    public DbSet<GameStateEntity> Games { get; set; } = default!;
}