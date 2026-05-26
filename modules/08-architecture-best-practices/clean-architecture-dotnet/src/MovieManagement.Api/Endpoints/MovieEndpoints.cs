using MovieManagement.Api.Infrastructure;
using MovieManagement.Application.Movies;

namespace MovieManagement.Api.Endpoints;

public static class MovieEndpoints
{
    public static IEndpointRouteBuilder MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/movies").WithTags("Movies");

        group.MapPost("/", async (CreateMovieRequest request, IMovieService service, CancellationToken cancellationToken) =>
        {
            var movie = await service.CreateAsync(request, cancellationToken);
            return Results.Created($"/movies/{movie.Id}", movie);
        }).AddEndpointFilter<ValidationFilter<CreateMovieRequest>>();

        group.MapGet("/", async (IMovieService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetAllAsync(cancellationToken)));

        group.MapGet("/{id:guid}", async (Guid id, IMovieService service, CancellationToken cancellationToken) =>
        {
            var movie = await service.GetByIdAsync(id, cancellationToken);
            return movie is null ? Results.NotFound() : Results.Ok(movie);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateMovieRequest request, IMovieService service, CancellationToken cancellationToken) =>
        {
            var updated = await service.UpdateAsync(id, request, cancellationToken);
            return updated ? Results.NoContent() : Results.NotFound();
        }).AddEndpointFilter<ValidationFilter<UpdateMovieRequest>>();

        group.MapPost("/{id:guid}/ratings", async (Guid id, AddRatingRequest request, IMovieService service, CancellationToken cancellationToken) =>
        {
            var rated = await service.AddRatingAsync(id, request, cancellationToken);
            return rated ? Results.NoContent() : Results.NotFound();
        }).AddEndpointFilter<ValidationFilter<AddRatingRequest>>();

        group.MapDelete("/{id:guid}", async (Guid id, IMovieService service, CancellationToken cancellationToken) =>
        {
            var deleted = await service.DeleteAsync(id, cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}
