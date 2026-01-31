using Microsoft.EntityFrameworkCore;
using Movies.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Example endpoint showing relationship queries
app.MapGet("/movies", async (MovieDbContext context) =>
{
    var movies = await context.Movies
        .Include(m => m.Director)
        .Include(m => m.Actors)
        .Include(m => m.Detail)
        .Select(m => new
        {
            m.Id,
            m.Title,
            m.Genre,
            m.Rating,
            m.ReleaseDate,
            Director = m.Director.Name,
            Actors = m.Actors.Select(a => a.Name).ToList(),
            HasDetail = m.Detail != null
        })
        .ToListAsync();
    
    return Results.Ok(movies);
});

app.MapGet("/directors/{id}/movies", async (Guid id, MovieDbContext context) =>
{
    var director = await context.Directors
        .Include(d => d.Movies)
        .FirstOrDefaultAsync(d => d.Id == id);
    
    if (director is null)
        return Results.NotFound();
    
    return Results.Ok(new
    {
        director.Name,
        Movies = director.Movies.Select(m => new { m.Id, m.Title, m.Rating })
    });
});

app.Run();
