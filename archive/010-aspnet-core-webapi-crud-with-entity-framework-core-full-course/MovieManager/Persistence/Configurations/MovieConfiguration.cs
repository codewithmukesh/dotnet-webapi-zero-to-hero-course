using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieManager.Entities;

namespace MovieManager.Persistence.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        // Define table name
        builder.ToTable("Movies");

        // Set primary key
        builder.HasKey(m => m.Id);

        // Configure properties
        builder.Property(m => m.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(m => m.Genre)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(m => m.ReleaseDate)
               .IsRequired();

        builder.Property(m => m.Rating)
               .IsRequired();

        // Configure Created and LastModified properties to be handled as immutable and modifiable timestamps
        builder.Property(m => m.Created)
               .IsRequired()
               .ValueGeneratedOnAdd();

        builder.Property(m => m.LastModified)
               .IsRequired()
               .ValueGeneratedOnUpdate();

        // Optional: Add indexes for better query performance
        builder.HasIndex(m => m.Title);
    }
}
