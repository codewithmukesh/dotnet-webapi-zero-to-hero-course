using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movies.Api.Domain;

namespace Movies.Api.Persistence.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("Movies");
        
        builder.HasKey(m => m.Id);
        
        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(m => m.Genre)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(m => m.Rating)
            .IsRequired();
            
        builder.Property(m => m.ReleaseDate)
            .IsRequired();
            
        // DirectorId foreign key is configured in DirectorConfiguration
        // One-to-One with MovieDetail is configured in MovieDetailConfiguration
        // Many-to-Many with Actors is configured in ActorConfiguration
    }
}
