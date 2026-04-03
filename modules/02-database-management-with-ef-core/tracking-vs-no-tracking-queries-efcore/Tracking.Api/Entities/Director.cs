namespace Tracking.Api.Entities;

public class Director
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<Movie> Movies { get; set; } = [];
}
