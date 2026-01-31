namespace Movies.Api.Domain;

public abstract class EntityBase
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastModifiedAt { get; protected set; }

    protected void UpdateLastModified()
    {
        LastModifiedAt = DateTimeOffset.UtcNow;
    }
}
