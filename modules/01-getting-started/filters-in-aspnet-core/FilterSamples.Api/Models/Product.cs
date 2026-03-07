namespace FilterSamples.Api.Models;

public record Product(int Id, string Name, decimal Price, string? Description = null);

public record CreateProductRequest(string Name, decimal Price, string? Description = null);
