using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movies.Api.Domain;

namespace Movies.Api.Persistence.Configurations;

public class DirectorConfiguration : IEntityTypeConfiguration<Director>
{
    public void Configure(EntityTypeBuilder<Director> builder)
    {
        builder.ToTable("Directors");
        
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(150);
            
        builder.Property(d => d.Country)
            .HasMaxLength(100);
            
        builder.Property(d => d.BirthDate)
            .IsRequired();
            
        // Configure One-to-Many relationship
        // One Director has many Movies
        // Each Movie has one Director
        builder.HasMany(d => d.Movies)
            .WithOne(m => m.Director)
            .HasForeignKey(m => m.DirectorId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent accidental cascade deletes
    }
}
