using Mapping.Benchmarks.Contracts;
using Mapping.Benchmarks.Domain;

namespace Mapping.Benchmarks.Mapping.Manual;

public static class ProductManualMapper
{
    public static ProductResponse ToResponse(this Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.StockQuantity,
        product.CreatedAt,
        product.UpdatedAt,
        product.Category.ToResponse(),
        product.Tags.ConvertAll(static t => t.ToResponse()));

    public static CategoryResponse ToResponse(this Category category) =>
        new(category.Id, category.Name, category.Slug);

    public static TagResponse ToResponse(this Tag tag) =>
        new(tag.Id, tag.Label);
}
