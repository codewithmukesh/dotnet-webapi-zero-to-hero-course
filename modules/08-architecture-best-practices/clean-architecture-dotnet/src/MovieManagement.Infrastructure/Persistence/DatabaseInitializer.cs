using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MovieManagement.Domain.Movies;

namespace MovieManagement.Infrastructure.Persistence;

// DEMO ONLY. Applies migrations and seeds sample data on startup so the solution
// runs with a single command. NEVER do this in production: migrating on startup
// races when more than one instance boots at once, and it runs schema changes
// outside your control. In production, apply migrations as an explicit, reviewed
// step in your deployment pipeline (or a dedicated one-off migration job), and
// seed reference data the same way.
public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        // On a fresh database EF logs an error reading "__EFMigrationsHistory"
        // before that table exists - it catches it and creates the schema. Expected.
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Database migrations applied.");

        await SeedAsync(context, logger, cancellationToken);
    }

    private static async Task SeedAsync(ApplicationDbContext context, ILogger logger, CancellationToken cancellationToken)
    {
        // Idempotent: if the table already has movies, leave the data untouched.
        if (await context.Movies.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Database already seeded, skipping.");
            return;
        }

        Movie[] movies =
        [
            Movie.Create("Inception", "Christopher Nolan", new DateOnly(2010, 7, 16), Genre.SciFi,
                "A thief who steals corporate secrets through dream-sharing technology is given the inverse task of planting an idea."),
            Movie.Create("The Shawshank Redemption", "Frank Darabont", new DateOnly(1994, 10, 14), Genre.Drama,
                "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency."),
            Movie.Create("The Dark Knight", "Christopher Nolan", new DateOnly(2008, 7, 18), Genre.Action,
                "Batman sets out to dismantle the remaining criminal organizations that plague Gotham, only to face the Joker."),
        ];

        context.Movies.AddRange(movies);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} movies.", movies.Length);
    }
}
