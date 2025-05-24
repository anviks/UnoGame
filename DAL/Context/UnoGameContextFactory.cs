using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DAL.Context;

public class UnoGameContextFactory : IDesignTimeDbContextFactory<UnoDbContext>
{
    public UnoDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // or use Path.Combine + Directory.GetParent if needed
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<UnoDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new UnoDbContext(optionsBuilder.Options);
    }
}