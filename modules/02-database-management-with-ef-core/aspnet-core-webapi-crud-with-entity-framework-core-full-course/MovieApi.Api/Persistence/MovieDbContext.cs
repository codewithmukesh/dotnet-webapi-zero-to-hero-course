using Microsoft.EntityFrameworkCore;
using MovieApi.Api.Models;

namespace MovieApi.Api.Persistence;

public class MovieDbContext(DbContextOptions<MovieDbContext> options) : DbContext(options)
{
    public DbSet<Movie> Movies => Set<Movie>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("app");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MovieDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                var sampleMovie = await context.Set<Movie>().FirstOrDefaultAsync(b => b.Title == "Sonic the Hedgehog 3", cancellationToken);
                if (sampleMovie == null)
                {
                    sampleMovie = Movie.Create("Sonic the Hedgehog 3", "Fantasy", new DateTimeOffset(new DateTime(2025, 1, 3), TimeSpan.Zero), 7);
                    await context.Set<Movie>().AddAsync(sampleMovie, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }
            })
            .UseSeeding((context, _) =>
            {
                var sampleMovie = context.Set<Movie>().FirstOrDefault(b => b.Title == "Sonic the Hedgehog 3");
                if (sampleMovie == null)
                {
                    sampleMovie = Movie.Create("Sonic the Hedgehog 3", "Fantasy", new DateTimeOffset(new DateTime(2025, 1, 3), TimeSpan.Zero), 7);
                    context.Set<Movie>().Add(sampleMovie);
                    context.SaveChanges();
                }
            });
    }
}
