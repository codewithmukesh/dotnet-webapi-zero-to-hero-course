namespace MigratingSwaggerToScalar.Api.Models;

/// <summary>Payload to create a new product.</summary>
/// <param name="Name">Display name of the product.</param>
/// <param name="Price">Unit price in USD.</param>
public record CreateProductRequest(string Name, decimal Price);

/// <summary>Payload to request a development JWT.</summary>
/// <param name="Username">The username to embed in the token.</param>
public record TokenRequest(string Username);
