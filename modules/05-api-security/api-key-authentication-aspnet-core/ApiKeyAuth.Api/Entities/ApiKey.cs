namespace ApiKeyAuth.Api.Entities;

public class ApiKey
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Prefix { get; set; } = default!;

    public string KeyHash { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string OwnerId { get; set; } = default!;

    public string Scopes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public DateTime? LastUsedAt { get; set; }

    public bool IsActive(TimeProvider time) =>
        RevokedAt is null && (ExpiresAt is null || ExpiresAt > time.GetUtcNow());
}
