# EF Core Relationships

This module demonstrates how to configure entity relationships in Entity Framework Core using Fluent API.

📚 **Article**: [EF Core Relationships – Complete Configuration Guide](https://codewithmukesh.com/blog/efcore-relationships/)

## Relationship Types Covered

| Type | Entities | Description |
|------|----------|-------------|
| **One-to-Many** | Director → Movies | One director directs many movies |
| **One-to-One** | Movie → MovieDetail | Each movie has optional detailed info |
| **Many-to-Many** | Movies ↔ Actors | Movies have many actors, actors appear in many movies |

## Key Files

```
Movies.Api/
├── Domain/
│   ├── EntityBase.cs         # Base entity with Id, timestamps
│   ├── Director.cs           # Director entity (One-to-Many principal)
│   ├── Movie.cs              # Movie entity (relationships configured)
│   ├── MovieDetail.cs        # MovieDetail entity (One-to-One dependent)
│   ├── Actor.cs              # Actor entity (Many-to-Many)
│   └── MovieActor.cs         # Join entity with payload data
├── Persistence/
│   ├── MovieDbContext.cs     # DbContext with all DbSets
│   └── Configurations/
│       ├── DirectorConfiguration.cs    # One-to-Many config
│       ├── MovieConfiguration.cs       # Movie properties
│       ├── MovieDetailConfiguration.cs # One-to-One config
│       └── ActorConfiguration.cs       # Many-to-Many with UsingEntity
└── Program.cs                # Example queries with Include
```

## Running the Project

```bash
# Start PostgreSQL (if using Docker)
docker run -d --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:17

# Navigate to project
cd Movies.Api

# Create migration
dotnet ef migrations add AddRelationships

# Apply migration
dotnet ef database update

# Run the API
dotnet run
```

## Relationship Configuration Highlights

### One-to-Many (Director → Movies)
```csharp
builder.HasMany(d => d.Movies)
    .WithOne(m => m.Director)
    .HasForeignKey(m => m.DirectorId)
    .OnDelete(DeleteBehavior.Restrict);
```

### One-to-One (Movie → MovieDetail)
```csharp
builder.HasOne(d => d.Movie)
    .WithOne(m => m.Detail)
    .HasForeignKey<MovieDetail>(d => d.MovieId)
    .OnDelete(DeleteBehavior.Cascade);
```

### Many-to-Many with Payload (Movies ↔ Actors)
```csharp
builder.HasMany(a => a.Movies)
    .WithMany(m => m.Actors)
    .UsingEntity<MovieActor>(
        l => l.HasOne(ma => ma.Movie).WithMany().HasForeignKey(ma => ma.MovieId),
        r => r.HasOne(ma => ma.Actor).WithMany().HasForeignKey(ma => ma.ActorId),
        j => {
            j.ToTable("MovieActors");
            j.HasKey(ma => new { ma.MovieId, ma.ActorId });
        });
```

## Delete Behaviors

| Behavior | Use When |
|----------|----------|
| **Restrict** | Prevent accidental deletes (Director with movies) |
| **Cascade** | Delete dependents automatically (MovieDetail with Movie) |
| **SetNull** | Orphan records allowed (nullable FK) |

## Learn More

- [EF Core Relationships Documentation](https://learn.microsoft.com/ef/core/modeling/relationships)
- [Fluent API Configuration Guide](https://codewithmukesh.com/blog/fluent-api-entity-configuration-efcore/)
