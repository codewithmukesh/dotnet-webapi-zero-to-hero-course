using MovieManagement.Domain.Movies;

namespace MovieManagement.Application.Movies;

public record CreateMovieRequest(
    string Title,
    string Director,
    DateOnly ReleaseDate,
    Genre Genre,
    string Synopsis);

public record UpdateMovieRequest(
    string Title,
    string Director,
    DateOnly ReleaseDate,
    Genre Genre,
    string Synopsis);

public record AddRatingRequest(int Score);

public record MovieResponse(
    Guid Id,
    string Title,
    string Director,
    DateOnly ReleaseDate,
    string Genre,
    string Synopsis,
    double? AverageRating,
    int RatingCount);
