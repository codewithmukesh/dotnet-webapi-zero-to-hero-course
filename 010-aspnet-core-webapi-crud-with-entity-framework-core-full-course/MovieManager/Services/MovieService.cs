using Microsoft.EntityFrameworkCore;
using MovieManager.DTOs;
using MovieManager.Entities;
using MovieManager.Persistence;

namespace MovieManager.Services;

public class MovieService : IMovieService
{
    private readonly MovieDbContext _dbContext;
    private readonly ILogger<MovieService> _logger;

    public MovieService(MovieDbContext dbContext, ILogger<MovieService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<MovieDto> CreateMovieAsync(CreateMovieDto command)
    {
        var movie = Movie.Create(command.Title, command.Genre, command.ReleaseDate, command.Rating);

        await _dbContext.Movies.AddAsync(movie);
        await _dbContext.SaveChangesAsync();

        return new MovieDto(
           movie.Id,
           movie.Title,
           movie.Genre,
           movie.ReleaseDate,
           movie.Rating
        );
    }

    public async Task DeleteMovieAsync(Guid id)
    {
        var movieToDelete = await _dbContext.Movies.FindAsync(id);
        if (movieToDelete != null)
        {
            _dbContext.Movies.Remove(movieToDelete);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<MovieDto>> GetAllMoviesAsync()
    {
        return await _dbContext.Movies
            .AsNoTracking()
            .Select(movie => new MovieDto(
                movie.Id,
                movie.Title,
                movie.Genre,
                movie.ReleaseDate,
                movie.Rating
            ))
            .ToListAsync();
    }

    public async Task<MovieDto?> GetMovieByIdAsync(Guid id)
    {
        var movie = await _dbContext.Movies
                               .AsNoTracking()
                               .FirstOrDefaultAsync(m => m.Id == id);
        if (movie == null)
            return null;

        return new MovieDto(
            movie.Id,
            movie.Title,
            movie.Genre,
            movie.ReleaseDate,
            movie.Rating
        );
    }

    public async Task UpdateMovieAsync(Guid id, UpdateMovieDto command)
    {
        var movieToUpdate = await _dbContext.Movies.FindAsync(id);
        if (movieToUpdate is null)
            throw new ArgumentNullException($"Invalid Movie Id.");
        movieToUpdate.Update(command.Title, command.Genre, command.ReleaseDate, command.Rating);
        await _dbContext.SaveChangesAsync();
    }
}
