namespace MovieManager.DTOs;

public record MovieDto(Guid Id, string Title, string Genre, DateTimeOffset ReleaseDate, double Rating);