using Microsoft.EntityFrameworkCore;
using MovieApi.Api.DTOs;
using MovieApi.Api.Models;
using MovieApi.Api.Persistence;
using Movies.Api.Common;

namespace MovieApi.Api.Services;

public class MovieService(MovieDbContext dbContext, ILogger<MovieService> logger) : IMovieService
{
    public async Task<MovieDto> CreateMovieAsync(CreateMovieDto command)
    {
        var movie = Movie.Create(command.Title, command.Genre, command.ReleaseDate, command.Rating);

        await dbContext.Movies.AddAsync(movie);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Created movie {MovieId}: {Title}", movie.Id, movie.Title);

        return new MovieDto(movie.Id, movie.Title, movie.Genre, movie.ReleaseDate, movie.Rating);
    }

    public async Task<PagedResponse<MovieDto>> GetAllMoviesAsync(
        MovieQueryFilter filter, CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 50);

        var query = dbContext.Movies.AsNoTracking().AsQueryable();

        // 1. Apply search filter (reduces the dataset)
        query = query.ApplySearch(filter.Search);

        // 2. Count total records AFTER filtering, BEFORE pagination
        var totalRecords = await query.CountAsync(cancellationToken);

        // 3. Apply sorting (default to Title if not specified)
        query = query.ApplySort(
            string.IsNullOrWhiteSpace(filter.SortBy) ? "Title" : filter.SortBy);

        // 4. Apply pagination and project to DTOs
        var movies = await query
            .ApplyPagination(pageNumber, pageSize)
            .Select(m => new MovieDto(m.Id, m.Title, m.Genre, m.ReleaseDate, m.Rating))
            .ToListAsync(cancellationToken);

        return new PagedResponse<MovieDto>
        {
            Data = movies,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
        };
    }

    public async Task<MovieDto?> GetMovieByIdAsync(Guid id)
    {
        var movie = await dbContext.Movies
                               .AsNoTracking()
                               .FirstOrDefaultAsync(m => m.Id == id);
        if (movie == null)
            return null;

        return new MovieDto(movie.Id, movie.Title, movie.Genre, movie.ReleaseDate, movie.Rating);
    }

    public async Task UpdateMovieAsync(Guid id, UpdateMovieDto command)
    {
        var movieToUpdate = await dbContext.Movies.FindAsync(id);
        if (movieToUpdate is null)
            throw new ArgumentNullException($"Invalid Movie Id.");
        movieToUpdate.Update(command.Title, command.Genre, command.ReleaseDate, command.Rating);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteMovieAsync(Guid id)
    {
        var movieToDelete = await dbContext.Movies.FindAsync(id);
        if (movieToDelete != null)
        {
            dbContext.Movies.Remove(movieToDelete);
            await dbContext.SaveChangesAsync();
        }
    }
}
