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
                    var movies = GetSeedMovies();
                    await context.Set<Movie>().AddRangeAsync(movies, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }
            })
            .UseSeeding((context, _) =>
            {
                if (!context.Set<Movie>().Any())
                {
                    var movies = GetSeedMovies();
                    context.Set<Movie>().AddRange(movies);
                    context.SaveChanges();
                }
            });
    }

    private static List<Movie> GetSeedMovies() =>
    [
        Movie.Create("The Shawshank Redemption", "Drama", new DateTimeOffset(1994, 9, 23, 0, 0, 0, TimeSpan.Zero), 9.3),
        Movie.Create("The Godfather", "Crime", new DateTimeOffset(1972, 3, 24, 0, 0, 0, TimeSpan.Zero), 9.2),
        Movie.Create("The Dark Knight", "Action", new DateTimeOffset(2008, 7, 18, 0, 0, 0, TimeSpan.Zero), 9.0),
        Movie.Create("The Lord of the Rings: The Return of the King", "Fantasy", new DateTimeOffset(2003, 12, 17, 0, 0, 0, TimeSpan.Zero), 9.0),
        Movie.Create("Pulp Fiction", "Crime", new DateTimeOffset(1994, 10, 14, 0, 0, 0, TimeSpan.Zero), 8.9),
        Movie.Create("Forrest Gump", "Drama", new DateTimeOffset(1994, 7, 6, 0, 0, 0, TimeSpan.Zero), 8.8),
        Movie.Create("Inception", "Sci-Fi", new DateTimeOffset(2010, 7, 16, 0, 0, 0, TimeSpan.Zero), 8.8),
        Movie.Create("Fight Club", "Drama", new DateTimeOffset(1999, 10, 15, 0, 0, 0, TimeSpan.Zero), 8.8),
        Movie.Create("The Matrix", "Sci-Fi", new DateTimeOffset(1999, 3, 31, 0, 0, 0, TimeSpan.Zero), 8.7),
        Movie.Create("Interstellar", "Sci-Fi", new DateTimeOffset(2014, 11, 7, 0, 0, 0, TimeSpan.Zero), 8.7),
        Movie.Create("Dune: Part Two", "Sci-Fi", new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero), 8.6),
        Movie.Create("Gladiator", "Action", new DateTimeOffset(2000, 5, 5, 0, 0, 0, TimeSpan.Zero), 8.5),
        Movie.Create("The Lion King", "Animation", new DateTimeOffset(1994, 6, 24, 0, 0, 0, TimeSpan.Zero), 8.5),
        Movie.Create("Oppenheimer", "Drama", new DateTimeOffset(2023, 7, 21, 0, 0, 0, TimeSpan.Zero), 8.5),
        Movie.Create("Parasite", "Thriller", new DateTimeOffset(2019, 5, 30, 0, 0, 0, TimeSpan.Zero), 8.5),
        Movie.Create("Jurassic Park", "Sci-Fi", new DateTimeOffset(1993, 6, 11, 0, 0, 0, TimeSpan.Zero), 8.2),
        Movie.Create("Spider-Man: No Way Home", "Action", new DateTimeOffset(2021, 12, 17, 0, 0, 0, TimeSpan.Zero), 8.2),
        Movie.Create("The Avengers", "Action", new DateTimeOffset(2012, 5, 4, 0, 0, 0, TimeSpan.Zero), 8.0),
        Movie.Create("Titanic", "Romance", new DateTimeOffset(1997, 12, 19, 0, 0, 0, TimeSpan.Zero), 7.9),
        Movie.Create("Everything Everywhere All at Once", "Sci-Fi", new DateTimeOffset(2022, 3, 25, 0, 0, 0, TimeSpan.Zero), 7.8),
    ];
}
