using System.ComponentModel.DataAnnotations;

namespace MinimalApis.ProductCatalog.Api.Models;

public record CreateProductRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    [Required, StringLength(50, MinimumLength = 1)] string Category,
    [Range(0.01, 999999.99)] decimal Price,
    [Range(0, int.MaxValue)] int Stock
);

public record UpdateProductRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    [Required, StringLength(50, MinimumLength = 1)] string Category,
    [Range(0.01, 999999.99)] decimal Price,
    [Range(0, int.MaxValue)] int Stock
);

public record UpdateStockRequest(
    [Range(0, int.MaxValue)] int Stock
);
