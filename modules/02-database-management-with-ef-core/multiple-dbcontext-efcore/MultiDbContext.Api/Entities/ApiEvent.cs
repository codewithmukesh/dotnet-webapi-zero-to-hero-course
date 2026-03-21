namespace MultiDbContext.Api.Entities;

public class ApiEvent
{
    public Guid Id { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
