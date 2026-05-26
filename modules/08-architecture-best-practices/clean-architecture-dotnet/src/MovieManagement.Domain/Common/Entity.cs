namespace MovieManagement.Domain.Common;

public abstract class Entity
{
    // Guid.CreateVersion7 generates sequential GUIDs, which index far better
    // in PostgreSQL than the random GUIDs the old Guid.NewGuid produced.
    public Guid Id { get; protected set; } = Guid.CreateVersion7();
}
