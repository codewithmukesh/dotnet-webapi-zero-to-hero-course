using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MovieApi.Api.DTOs;
using MovieApi.Api.Persistence;
using MovieApi.Api.Services;
using Movies.Api.Common;

namespace MovieApi.Api.Endpoints;

public static class MovieEndpoints
{
    public static void MapMovieEndpoints(this IEndpointRouteBuilder routes)
    {
        var movieApi = routes.MapGroup("/api/movies").WithTags("Movies");

        // Offset pagination: GET /api/movies?pageNumber=1&pageSize=5&sortBy=rating desc&search=action
        movieApi.MapGet("/", async (
            [AsParameters] MovieQueryFilter filter,
            IMovieService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetAllMoviesAsync(filter, cancellationToken);
            return TypedResults.Ok(result);
        });

        // Keyset (cursor) pagination: GET /api/movies/cursor?after=2026-01-01T00:00:00Z&pageSize=10
        movieApi.MapGet("/cursor", async (
            MovieDbContext db,
            DateTimeOffset? after,
            int pageSize = 10,
            CancellationToken cancellationToken = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = db.Movies
                .AsNoTracking()
                .OrderByDescending(m => m.Created)
                .AsQueryable();

            if (after.HasValue)
            {
                query = query.Where(m => m.Created < after.Value);
            }

            var items = await query
                .Take(pageSize + 1)
                .Select(m => new MovieDto(m.Id, m.Title, m.Genre, m.ReleaseDate, m.Rating))
                .ToListAsync(cancellationToken);

            var hasMore = items.Count > pageSize;
            var data = hasMore ? items[..pageSize] : items;

            return TypedResults.Ok(new
            {
                Data = data,
                HasNextPage = hasMore
            });
        });

        movieApi.MapGet("/{id}", async (IMovieService service, Guid id) =>
        {
            var movie = await service.GetMovieByIdAsync(id);

            return movie is null
                ? (IResult)TypedResults.NotFound(new { Message = $"Movie with ID {id} not found." })
                : TypedResults.Ok(movie);
        });

        movieApi.MapPost("/", async (IMovieService service, CreateMovieDto command) =>
        {
            var movie = await service.CreateMovieAsync(command);
            return TypedResults.Created($"/api/movies/{movie.Id}", movie);
        });

        movieApi.MapPut("/{id}", async (IMovieService service, Guid id, UpdateMovieDto command) =>
        {
            await service.UpdateMovieAsync(id, command);
            return TypedResults.NoContent();
        });

        movieApi.MapDelete("/{id}", async (IMovieService service, Guid id) =>
        {
            await service.DeleteMovieAsync(id);
            return TypedResults.NoContent();
        });
    }
}
