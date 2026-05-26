using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieManagement.Domain.Movies;

namespace MovieManagement.Infrastructure.Persistence.Configurations;

public sealed class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("movies");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Director)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(m => m.Synopsis)
            .HasMaxLength(2000);

        // Store the enum as a readable string column instead of an int.
        builder.Property(m => m.Genre)
            .HasConversion<string>()
            .HasMaxLength(40);
    }
}
