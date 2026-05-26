using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MovieManagement.Infrastructure.Persistence;

// Used only by the EF Core CLI (dotnet ef) to build the DbContext at design
// time, so you can add migrations without spinning up Aspire. At runtime,
// Aspire supplies the real connection string.
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=moviesdb;Username=postgres;Password=postgres")
            .Options;

        return new ApplicationDbContext(options);
    }
}
