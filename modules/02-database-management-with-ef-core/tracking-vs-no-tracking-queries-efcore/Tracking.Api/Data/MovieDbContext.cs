using Microsoft.EntityFrameworkCore;
using Tracking.Api.Entities;

namespace Tracking.Api.Data;

public class MovieDbContext(DbContextOptions<MovieDbContext> options) : DbContext(options)
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Director> Directors => Set<Director>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Title).HasMaxLength(200).IsRequired();
            entity.Property(m => m.Genre).HasMaxLength(100).IsRequired();
            entity.Property(m => m.Rating).HasPrecision(3, 1);
            entity.HasOne(m => m.Director)
                .WithMany(d => d.Movies)
                .HasForeignKey(m => m.DirectorId);
        });

        modelBuilder.Entity<Director>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).HasMaxLength(200).IsRequired();
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var director1 = new Director { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), Name = "Christopher Nolan" };
        var director2 = new Director { Id = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"), Name = "Denis Villeneuve" };
        var director3 = new Director { Id = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"), Name = "Martin Scorsese" };

        modelBuilder.Entity<Director>().HasData(director1, director2, director3);

        modelBuilder.Entity<Movie>().HasData(
            new Movie { Id = Guid.NewGuid(), Title = "Inception", Genre = "Sci-Fi", Rating = 8.8m, ReleaseYear = 2010, DirectorId = director1.Id },
            new Movie { Id = Guid.NewGuid(), Title = "The Dark Knight", Genre = "Action", Rating = 9.0m, ReleaseYear = 2008, DirectorId = director1.Id },
            new Movie { Id = Guid.NewGuid(), Title = "Interstellar", Genre = "Sci-Fi", Rating = 8.7m, ReleaseYear = 2014, DirectorId = director1.Id },
            new Movie { Id = Guid.NewGuid(), Title = "Tenet", Genre = "Sci-Fi", Rating = 7.3m, ReleaseYear = 2020, DirectorId = director1.Id },
            new Movie { Id = Guid.NewGuid(), Title = "Dune", Genre = "Sci-Fi", Rating = 8.0m, ReleaseYear = 2021, DirectorId = director2.Id },
            new Movie { Id = Guid.NewGuid(), Title = "Dune: Part Two", Genre = "Sci-Fi", Rating = 8.8m, ReleaseYear = 2024, DirectorId = director2.Id },
            new Movie { Id = Guid.NewGuid(), Title = "Blade Runner 2049", Genre = "Sci-Fi", Rating = 8.0m, ReleaseYear = 2017, DirectorId = director2.Id },
            new Movie { Id = Guid.NewGuid(), Title = "Arrival", Genre = "Sci-Fi", Rating = 7.9m, ReleaseYear = 2016, DirectorId = director2.Id },
            new Movie { Id = Guid.NewGuid(), Title = "The Wolf of Wall Street", Genre = "Drama", Rating = 8.2m, ReleaseYear = 2013, DirectorId = director3.Id },
            new Movie { Id = Guid.NewGuid(), Title = "Goodfellas", Genre = "Crime", Rating = 8.7m, ReleaseYear = 1990, DirectorId = director3.Id },
            new Movie { Id = Guid.NewGuid(), Title = "The Departed", Genre = "Crime", Rating = 8.5m, ReleaseYear = 2006, DirectorId = director3.Id },
            new Movie { Id = Guid.NewGuid(), Title = "Killers of the Flower Moon", Genre = "Drama", Rating = 7.6m, ReleaseYear = 2023, DirectorId = director3.Id }
        );
    }
}
