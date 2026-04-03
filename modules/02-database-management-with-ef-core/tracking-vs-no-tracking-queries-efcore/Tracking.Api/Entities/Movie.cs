namespace Tracking.Api.Entities;

public class Movie
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Genre { get; set; } = default!;
    public decimal Rating { get; set; }
    public int ReleaseYear { get; set; }
    public Guid DirectorId { get; set; }
    public Director Director { get; set; } = default!;
}
