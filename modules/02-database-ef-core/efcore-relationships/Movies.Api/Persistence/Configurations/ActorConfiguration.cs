using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movies.Api.Domain;

namespace Movies.Api.Persistence.Configurations;

public class ActorConfiguration : IEntityTypeConfiguration<Actor>
{
    public void Configure(EntityTypeBuilder<Actor> builder)
    {
        builder.ToTable("Actors");
        
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(150);
            
        // Configure Many-to-Many relationship with explicit join entity
        // This allows storing additional data (Role, CharacterName, BillingOrder)
        builder.HasMany(a => a.Movies)
            .WithMany(m => m.Actors)
            .UsingEntity<MovieActor>(
                // Configure the Movie side of the relationship
                l => l.HasOne(ma => ma.Movie)
                      .WithMany()
                      .HasForeignKey(ma => ma.MovieId),
                // Configure the Actor side of the relationship
                r => r.HasOne(ma => ma.Actor)
                      .WithMany()
                      .HasForeignKey(ma => ma.ActorId),
                // Configure the join table itself
                j =>
                {
                    j.ToTable("MovieActors");
                    j.HasKey(ma => new { ma.MovieId, ma.ActorId });
                    j.Property(ma => ma.Role).HasMaxLength(50);
                    j.Property(ma => ma.CharacterName).HasMaxLength(200);
                });
    }
}
