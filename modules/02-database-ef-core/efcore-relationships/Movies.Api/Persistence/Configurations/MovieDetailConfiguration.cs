using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movies.Api.Domain;

namespace Movies.Api.Persistence.Configurations;

public class MovieDetailConfiguration : IEntityTypeConfiguration<MovieDetail>
{
    public void Configure(EntityTypeBuilder<MovieDetail> builder)
    {
        builder.ToTable("MovieDetails");
        
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Synopsis)
            .HasMaxLength(2000);
            
        builder.Property(d => d.TrailerUrl)
            .HasMaxLength(500);
            
        builder.Property(d => d.Language)
            .HasMaxLength(50);
            
        // Configure One-to-One relationship
        // MovieDetail has one Movie
        // Movie has one (optional) MovieDetail
        builder.HasOne(d => d.Movie)
            .WithOne(m => m.Detail)
            .HasForeignKey<MovieDetail>(d => d.MovieId)
            .OnDelete(DeleteBehavior.Cascade); // Delete movie -> delete its details
    }
}
