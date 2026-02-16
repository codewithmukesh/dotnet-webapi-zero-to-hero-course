namespace MovieApi.Api.Models;

public sealed class Movie : EntityBase
{
    public string Title { get; private set; }
    public string Genre { get; private set; }
    public DateTimeOffset ReleaseDate { get; private set; }
    public double Rating { get; private set; }

    // Private constructor for ORM frameworks
    private Movie()
    {
        Title = string.Empty;
        Genre = string.Empty;
    }
    private Movie(string title, string genre, DateTimeOffset releaseDate, double rating)
    {
        Title = title;
        Genre = genre;
        ReleaseDate = releaseDate;
        Rating = rating;
    }

    public static Movie Create(string title, string genre, DateTimeOffset releaseDate, double rating)
    {
        ValidateInputs(title, genre, releaseDate, rating);
        return new Movie(title, genre, releaseDate, rating);
    }

    public void Update(string title, string genre, DateTimeOffset releaseDate, double rating)
    {
        ValidateInputs(title, genre, releaseDate, rating);

        Title = title;
        Genre = genre;
        ReleaseDate = releaseDate;
        Rating = rating;

        UpdateLastModified();
    }

    private static void ValidateInputs(string title, string genre, DateTimeOffset releaseDate, double rating)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(genre))
            throw new ArgumentException("Genre cannot be null or empty.", nameof(genre));

        if (releaseDate > DateTimeOffset.UtcNow)
            throw new ArgumentException("Release date cannot be in the future.", nameof(releaseDate));

        if (rating < 0 || rating > 10)
            throw new ArgumentException("Rating must be between 0 and 10.", nameof(rating));
    }
}
