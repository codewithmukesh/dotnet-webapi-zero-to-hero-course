using Microsoft.EntityFrameworkCore;
using MovieManagement.Application.Common;
using MovieManagement.Domain.Movies;

namespace MovieManagement.Application.Movies;

public sealed class MovieService(IApplicationDbContext context) : IMovieService
{
    public async Task<MovieResponse> CreateAsync(CreateMovieRequest request, CancellationToken cancellationToken)
    {
        var movie = Movie.Create(
            request.Title,
            request.Director,
            request.ReleaseDate,
            request.Genre,
            request.Synopsis);

        context.Movies.Add(movie);
        await context.SaveChangesAsync(cancellationToken);

        return movie.ToResponse();
    }

    public async Task<MovieResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var movie = await context.Movies
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        return movie?.ToResponse();
    }

    public async Task<IReadOnlyList<MovieResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        // AsNoTracking skips change-tracking on read-only queries, which cuts
        // allocations and runs faster. Use it on every query that only reads.
        var movies = await context.Movies
            .AsNoTracking()
            .OrderByDescending(m => m.ReleaseDate)
            .ToListAsync(cancellationToken);

        return movies.Select(m => m.ToResponse()).ToList();
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateMovieRequest request, CancellationToken cancellationToken)
    {
        var movie = await context.Movies.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (movie is null)
        {
            return false;
        }

        movie.UpdateDetails(
            request.Title,
            request.Director,
            request.ReleaseDate,
            request.Genre,
            request.Synopsis);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AddRatingAsync(Guid id, AddRatingRequest request, CancellationToken cancellationToken)
    {
        var movie = await context.Movies.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (movie is null)
        {
            return false;
        }

        // The Movie checks the score and updates its own average. If the score
        // is out of range, the domain throws and the API turns it into a 400.
        movie.AddRating(request.Score);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var movie = await context.Movies.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (movie is null)
        {
            return false;
        }

        context.Movies.Remove(movie);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
