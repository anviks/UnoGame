using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Context;

public class UnoDbContext: DbContext
{
    public DbSet<GameStateEntity> Games { get; set; } = default!;

    public UnoDbContext(DbContextOptions<UnoDbContext> options) : base(options)
    {
    }
}