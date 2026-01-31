namespace Movies.Api.Domain;

public class MovieDetail : EntityBase
{
    public string Synopsis { get; private set; } = string.Empty;
    public string TrailerUrl { get; private set; } = string.Empty;
    public int RuntimeMinutes { get; private set; }
    public string Language { get; private set; } = string.Empty;
    
    // Foreign key and navigation to Movie
    public Guid MovieId { get; private set; }
    public Movie Movie { get; private set; } = null!;
    
    private MovieDetail() { }
    
    private MovieDetail(Guid movieId, string synopsis, string trailerUrl, int runtimeMinutes, string language)
    {
        MovieId = movieId;
        Synopsis = synopsis;
        TrailerUrl = trailerUrl;
        RuntimeMinutes = runtimeMinutes;
        Language = language;
    }
    
    public static MovieDetail Create(Guid movieId, string synopsis, string trailerUrl, int runtimeMinutes, string language)
    {
        if (movieId == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be empty.", nameof(movieId));
            
        if (runtimeMinutes <= 0)
            throw new ArgumentException("Runtime must be positive.", nameof(runtimeMinutes));
            
        return new MovieDetail(movieId, synopsis, trailerUrl, runtimeMinutes, language);
    }
    
    public void Update(string synopsis, string trailerUrl, int runtimeMinutes, string language)
    {
        if (runtimeMinutes <= 0)
            throw new ArgumentException("Runtime must be positive.", nameof(runtimeMinutes));
            
        Synopsis = synopsis;
        TrailerUrl = trailerUrl;
        RuntimeMinutes = runtimeMinutes;
        Language = language;
        UpdateLastModified();
    }
}
