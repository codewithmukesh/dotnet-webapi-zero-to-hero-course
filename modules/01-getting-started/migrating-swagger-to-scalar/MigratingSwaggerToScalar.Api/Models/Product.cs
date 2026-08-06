namespace MigratingSwaggerToScalar.Api.Models;

/// <summary>A product in the catalog.</summary>
/// <param name="Id">Unique identifier.</param>
/// <param name="Name">Display name.</param>
/// <param name="Price">Unit price in USD.</param>
public record Product(int Id, string Name, decimal Price);
