using MovieManagement.Domain.Movies;

namespace MovieManagement.Application.Movies;

internal static class MovieMappings
{
    public static MovieResponse ToResponse(this Movie movie) => new(
        movie.Id,
        movie.Title,
        movie.Director,
        movie.ReleaseDate,
        movie.Genre.ToString(),
        movie.Synopsis,
        movie.AverageRating,
        movie.RatingCount);
}
