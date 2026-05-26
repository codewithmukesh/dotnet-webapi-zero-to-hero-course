namespace MovieManagement.Application.Movies;

public interface IMovieService
{
    Task<MovieResponse> CreateAsync(CreateMovieRequest request, CancellationToken cancellationToken);
    Task<MovieResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<MovieResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Guid id, UpdateMovieRequest request, CancellationToken cancellationToken);
    Task<bool> AddRatingAsync(Guid id, AddRatingRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
