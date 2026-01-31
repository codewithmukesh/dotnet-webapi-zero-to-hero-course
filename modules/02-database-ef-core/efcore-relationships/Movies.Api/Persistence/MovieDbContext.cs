using Microsoft.EntityFrameworkCore;
using Movies.Api.Domain;

namespace Movies.Api.Persistence;

public class MovieDbContext : DbContext
{
    public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Director> Directors => Set<Director>();
    public DbSet<Actor> Actors => Set<Actor>();
    public DbSet<MovieDetail> MovieDetails => Set<MovieDetail>();
    public DbSet<MovieActor> MovieActors => Set<MovieActor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MovieDbContext).Assembly);
    }
}
