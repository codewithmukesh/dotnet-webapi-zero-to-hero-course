namespace Mapping.Api.Contracts;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    CategoryResponse Category,
    IReadOnlyList<TagResponse> Tags);

public sealed record CategoryResponse(int Id, string Name, string Slug);

public sealed record TagResponse(int Id, string Label);
