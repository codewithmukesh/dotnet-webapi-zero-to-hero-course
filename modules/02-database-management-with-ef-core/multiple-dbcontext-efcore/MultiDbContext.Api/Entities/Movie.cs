namespace MultiDbContext.Api.Entities;

public class Movie
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public int ReleaseYear { get; set; }
}
