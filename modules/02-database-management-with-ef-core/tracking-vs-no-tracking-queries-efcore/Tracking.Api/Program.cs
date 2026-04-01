using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Tracking.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

var app = builder.Build();

// Ensure database is created (for development only)
await using (var serviceScope = app.Services.CreateAsyncScope())
await using (var dbContext = serviceScope.ServiceProvider.GetRequiredService<MovieDbContext>())
{
    await dbContext.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// GET all movies - no-tracking by default (read-only)
app.MapGet("/api/movies", async (MovieDbContext db, CancellationToken ct) =>
{
    var movies = await db.Movies
        .OrderByDescending(m => m.Rating)
        .ToListAsync(ct);

    return Results.Ok(movies);
});

// GET movie by ID - no-tracking by default (read-only)
app.MapGet("/api/movies/{id:guid}", async (Guid id, MovieDbContext db, CancellationToken ct) =>
{
    var movie = await db.Movies
        .Include(m => m.Director)
        .FirstOrDefaultAsync(m => m.Id == id, ct);

    return movie is null ? Results.NotFound() : Results.Ok(movie);
});

// GET movies with directors - AsNoTrackingWithIdentityResolution
// Directors are shared across movies, so identity resolution prevents duplicates
app.MapGet("/api/movies/with-directors", async (MovieDbContext db, CancellationToken ct) =>
{
    var movies = await db.Movies
        .AsNoTrackingWithIdentityResolution()
        .Include(m => m.Director)
        .OrderByDescending(m => m.Rating)
        .ToListAsync(ct);

    return Results.Ok(movies);
});

// GET movie summaries - projection (never tracked)
app.MapGet("/api/movies/summaries", async (MovieDbContext db, CancellationToken ct) =>
{
    var summaries = await db.Movies
        .Select(m => new { m.Title, m.Genre, m.Rating, Director = m.Director.Name })
        .OrderByDescending(m => m.Rating)
        .ToListAsync(ct);

    return Results.Ok(summaries);
});

// PUT update movie - explicitly uses AsTracking (default is NoTracking)
app.MapPut("/api/movies/{id:guid}", async (Guid id, UpdateMovieRequest request,
    MovieDbContext db, CancellationToken ct) =>
{
    var movie = await db.Movies
        .AsTracking()
        .FirstOrDefaultAsync(m => m.Id == id, ct);

    if (movie is null) return Results.NotFound();

    movie.Title = request.Title;
    movie.Rating = request.Rating;

    await db.SaveChangesAsync(ct);
    return Results.Ok(movie);
});

// POST create movie - tracking is implicit for Add operations
app.MapPost("/api/movies", async (CreateMovieRequest request,
    MovieDbContext db, CancellationToken ct) =>
{
    var director = await db.Directors
        .AsTracking()
        .FirstOrDefaultAsync(d => d.Id == request.DirectorId, ct);

    if (director is null) return Results.BadRequest("Director not found.");

    var movie = new Tracking.Api.Entities.Movie
    {
        Title = request.Title,
        Genre = request.Genre,
        Rating = request.Rating,
        ReleaseYear = request.ReleaseYear,
        DirectorId = request.DirectorId
    };

    db.Movies.Add(movie);
    await db.SaveChangesAsync(ct);

    return Results.Created($"/api/movies/{movie.Id}", movie);
});

// DELETE movie - explicitly uses AsTracking
app.MapDelete("/api/movies/{id:guid}", async (Guid id, MovieDbContext db, CancellationToken ct) =>
{
    var movie = await db.Movies
        .AsTracking()
        .FirstOrDefaultAsync(m => m.Id == id, ct);

    if (movie is null) return Results.NotFound();

    db.Movies.Remove(movie);
    await db.SaveChangesAsync(ct);

    return Results.NoContent();
});

// GET directors
app.MapGet("/api/directors", async (MovieDbContext db, CancellationToken ct) =>
{
    var directors = await db.Directors
        .Include(d => d.Movies)
        .ToListAsync(ct);

    return Results.Ok(directors);
});

app.Run();

public record UpdateMovieRequest(string Title, decimal Rating);
public record CreateMovieRequest(string Title, string Genre, decimal Rating, int ReleaseYear, Guid DirectorId);
