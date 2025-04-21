using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL.Context;

public class UnoGameContextFactory : IDesignTimeDbContextFactory<UnoDbContext>
{
    public UnoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UnoDbContext>();
        optionsBuilder.UseSqlite($"Data Source={GameRepositoryDb.Instance.SavePath}");

        return new UnoDbContext(optionsBuilder.Options);
    }
}