using MovieManagement.Domain.Common;

namespace MovieManagement.Domain.Movies;

public sealed class Movie : Entity
{
    // EF Core needs a parameterless constructor. Keeping it private means the
    // rest of the application cannot create a Movie in an invalid state.
    private Movie()
    {
    }

    private Movie(string title, string director, DateOnly releaseDate, Genre genre, string synopsis)
    {
        Title = title;
        Director = director;
        ReleaseDate = releaseDate;
        Genre = genre;
        Synopsis = synopsis;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public string Title { get; private set; } = default!;
    public string Director { get; private set; } = default!;
    public DateOnly ReleaseDate { get; private set; }
    public Genre Genre { get; private set; }
    public string Synopsis { get; private set; } = default!;
    public double? AverageRating { get; private set; }
    public int RatingCount { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    // A factory method is the only way to build a Movie. It enforces the rules
    // that must always be true, so an invalid Movie can never exist.
    public static Movie Create(string title, string director, DateOnly releaseDate, Genre genre, string synopsis)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("A movie must have a title.");
        }

        if (string.IsNullOrWhiteSpace(director))
        {
            throw new DomainException("A movie must have a director.");
        }

        return new Movie(title.Trim(), director.Trim(), releaseDate, genre, synopsis?.Trim() ?? string.Empty);
    }

    public void UpdateDetails(string title, string director, DateOnly releaseDate, Genre genre, string synopsis)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("A movie must have a title.");
        }

        Title = title.Trim();
        Director = director.Trim();
        ReleaseDate = releaseDate;
        Genre = genre;
        Synopsis = synopsis?.Trim() ?? string.Empty;
    }

    // Behavior lives on the entity, not in a service. The running average is a
    // business rule, so the Movie owns it.
    public void AddRating(int score)
    {
        if (score is < 1 or > 10)
        {
            throw new DomainException("A rating must be between 1 and 10.");
        }

        var runningTotal = (AverageRating ?? 0) * RatingCount + score;
        RatingCount++;
        AverageRating = Math.Round(runningTotal / RatingCount, 2);
    }
}
