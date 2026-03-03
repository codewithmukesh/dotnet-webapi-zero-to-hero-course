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
                if (!await context.Set<Movie>().AnyAsync(cancellationToken))
                {
                    var movies = new[]
                    {
                        Movie.Create("The Shawshank Redemption", "Drama",
                            new DateTimeOffset(new DateTime(1994, 9, 23), TimeSpan.Zero), 9.3),
                        Movie.Create("The Dark Knight", "Action",
                            new DateTimeOffset(new DateTime(2008, 7, 18), TimeSpan.Zero), 9.0),
                        Movie.Create("Inception", "Sci-Fi",
                            new DateTimeOffset(new DateTime(2010, 7, 16), TimeSpan.Zero), 8.8),
                        Movie.Create("Interstellar", "Sci-Fi",
                            new DateTimeOffset(new DateTime(2014, 11, 7), TimeSpan.Zero), 8.7),
                        Movie.Create("The Matrix", "Sci-Fi",
                            new DateTimeOffset(new DateTime(1999, 3, 31), TimeSpan.Zero), 8.7)
                    };

                    await context.Set<Movie>().AddRangeAsync(movies, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }
            })
            .UseSeeding((context, _) =>
            {
                if (!context.Set<Movie>().Any())
                {
                    var movies = new[]
                    {
                        Movie.Create("The Shawshank Redemption", "Drama",
                            new DateTimeOffset(new DateTime(1994, 9, 23), TimeSpan.Zero), 9.3),
                        Movie.Create("The Dark Knight", "Action",
                            new DateTimeOffset(new DateTime(2008, 7, 18), TimeSpan.Zero), 9.0),
                        Movie.Create("Inception", "Sci-Fi",
                            new DateTimeOffset(new DateTime(2010, 7, 16), TimeSpan.Zero), 8.8),
                        Movie.Create("Interstellar", "Sci-Fi",
                            new DateTimeOffset(new DateTime(2014, 11, 7), TimeSpan.Zero), 8.7),
                        Movie.Create("The Matrix", "Sci-Fi",
                            new DateTimeOffset(new DateTime(1999, 3, 31), TimeSpan.Zero), 8.7)
                    };

                    context.Set<Movie>().AddRange(movies);
                    context.SaveChanges();
                }
            });
    }
}
