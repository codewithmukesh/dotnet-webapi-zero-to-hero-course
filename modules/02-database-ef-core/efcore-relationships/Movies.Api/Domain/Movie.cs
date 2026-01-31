namespace Movies.Api.Domain;

public class Movie : EntityBase
{
    public string Title { get; private set; } = string.Empty;
    public string Genre { get; private set; } = string.Empty;
    public DateTimeOffset ReleaseDate { get; private set; }
    public double Rating { get; private set; }
    
    // One-to-Many: Each movie has one director
    public Guid DirectorId { get; private set; }
    public Director Director { get; private set; } = null!;
    
    // One-to-One: Movie optionally has detailed info
    public MovieDetail? Detail { get; private set; }
    
    // Many-to-Many: Movie has many actors
    public ICollection<Actor> Actors { get; private set; } = new List<Actor>();

    private Movie() { }

    private Movie(string title, string genre, DateTimeOffset releaseDate, double rating)
    {
        Title = title;
        Genre = genre;
        ReleaseDate = releaseDate;
        Rating = rating;
    }

    public static Movie Create(string title, string genre, DateTimeOffset releaseDate, double rating, Guid directorId)
    {
        ValidateInputs(title, genre, releaseDate, rating);
        
        if (directorId == Guid.Empty)
            throw new ArgumentException("Director ID cannot be empty.", nameof(directorId));
        
        return new Movie(title, genre, releaseDate, rating) { DirectorId = directorId };
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
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(genre))
            throw new ArgumentException("Genre cannot be empty.", nameof(genre));
        if (releaseDate > DateTimeOffset.UtcNow.AddYears(5))
            throw new ArgumentException("Release date cannot be more than 5 years in the future.", nameof(releaseDate));
        if (rating < 0 || rating > 10)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 0 and 10.");
    }
}
