using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieApi.Api.Models;

namespace MovieApi.Api.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Genre).HasMaxLength(100).IsRequired();

        builder.HasIndex(m => m.Genre);
        builder.HasIndex(m => m.Rating);
        builder.HasIndex(m => m.Created);
        builder.HasIndex(m => new { m.Genre, m.Rating });
    }
}
