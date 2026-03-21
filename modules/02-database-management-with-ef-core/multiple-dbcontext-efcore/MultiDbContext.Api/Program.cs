using Microsoft.EntityFrameworkCore;
using MultiDbContext.Api.Data;
using MultiDbContext.Api.Entities;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MovieDb")));

builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AnalyticsDb")));

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// Movie endpoints - uses MovieDbContext
app.MapGet("/api/movies", async (MovieDbContext db, CancellationToken ct) =>
{
    var movies = await db.Movies.AsNoTracking().ToListAsync(ct);
    return Results.Ok(movies);
});

app.MapGet("/api/movies/{id:guid}", async (Guid id,
    MovieDbContext db, AnalyticsDbContext analytics, CancellationToken ct) =>
{
    var movie = await db.Movies.FindAsync([id], ct);
    if (movie is null) return Results.NotFound();

    analytics.ApiEvents.Add(new ApiEvent
    {
        Id = Guid.CreateVersion7(),
        Endpoint = $"/api/movies/{id}",
        Method = "GET",
        StatusCode = 200,
        DurationMs = 0
    });
    await analytics.SaveChangesAsync(ct);

    return Results.Ok(movie);
});

app.MapPost("/api/movies", async (CreateMovieRequest request,
    MovieDbContext db, CancellationToken ct) =>
{
    var movie = new Movie
    {
        Id = Guid.CreateVersion7(),
        Title = request.Title,
        Genre = request.Genre,
        Rating = request.Rating,
        ReleaseYear = request.ReleaseYear
    };
    db.Movies.Add(movie);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/movies/{movie.Id}", movie);
});

// Analytics endpoints - uses AnalyticsDbContext
app.MapGet("/api/analytics/events", async (AnalyticsDbContext analytics, CancellationToken ct) =>
{
    var events = await analytics.ApiEvents
        .AsNoTracking()
        .OrderByDescending(e => e.OccurredAt)
        .Take(100)
        .ToListAsync(ct);
    return Results.Ok(events);
});

// Cross-context transaction example (same database only)
app.MapPost("/api/movies/with-event", async (CreateMovieRequest request,
    MovieDbContext movieDb, AnalyticsDbContext analyticsDb, CancellationToken ct) =>
{
    var connection = movieDb.Database.GetDbConnection();
    await connection.OpenAsync(ct);

    await using var transaction = await connection.BeginTransactionAsync(ct);

    try
    {
        analyticsDb.Database.SetDbConnection(connection);
        await analyticsDb.Database.UseTransactionAsync(transaction, ct);

        var movie = new Movie
        {
            Id = Guid.CreateVersion7(),
            Title = request.Title,
            Genre = request.Genre,
            Rating = request.Rating,
            ReleaseYear = request.ReleaseYear
        };
        movieDb.Movies.Add(movie);
        await movieDb.SaveChangesAsync(ct);

        analyticsDb.ApiEvents.Add(new ApiEvent
        {
            Id = Guid.CreateVersion7(),
            Endpoint = "/api/movies",
            Method = "POST",
            StatusCode = 201,
            DurationMs = 0
        });
        await analyticsDb.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);
        return Results.Created($"/api/movies/{movie.Id}", movie);
    }
    catch
    {
        await transaction.RollbackAsync(ct);
        throw;
    }
});

app.Run();

public record CreateMovieRequest(string Title, string Genre, decimal Rating, int ReleaseYear);
