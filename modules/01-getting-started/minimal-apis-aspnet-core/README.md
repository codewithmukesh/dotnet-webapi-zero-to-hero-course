# Minimal API Endpoints in ASP.NET Core

Complete Product Catalog API demonstrating Minimal APIs in .NET 10.

## Article

**[Minimal API Endpoints in ASP.NET Core - Complete Guide for .NET 10](https://codewithmukesh.com/blog/minimal-apis-aspnet-core/)**

## Course

**[.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)** - The complete FREE course for .NET Web API development.

## What You'll Learn

- Route handlers for all HTTP verbs
- Parameter binding (route, query, body, headers)
- TypedResults for strongly typed responses
- Route groups for endpoint organization
- Endpoint filters for cross-cutting concerns
- Built-in validation with `AddValidation()` (.NET 10)
- OpenAPI 3.1 with Scalar UI

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2026 / VS Code / Rider

## Running the API

```bash
cd MinimalApis.ProductCatalog.Api
dotnet run
```

Navigate to `http://localhost:5000/scalar/v1` for the API documentation.

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Get all products (with filtering) |
| GET | `/api/products/{id}` | Get product by ID |
| GET | `/api/products/category/{category}` | Get products by category |
| GET | `/api/products/stats` | Get product statistics |
| POST | `/api/products` | Create a new product |
| PUT | `/api/products/{id}` | Update a product |
| PATCH | `/api/products/{id}/stock` | Update product stock |
| DELETE | `/api/products/{id}` | Delete a product |

## Project Structure

```
MinimalApis.ProductCatalog.Api/
├── Program.cs              # All endpoints and configuration
├── Models/
│   ├── Product.cs          # Product record
│   └── Requests.cs         # Request DTOs with validation
├── Filters/
│   └── LoggingFilter.cs    # Custom endpoint filter
├── appsettings.json
└── appsettings.Development.json
```

## Key Features Demonstrated

### TypedResults

```csharp
productsGroup.MapGet("/{id:int}", Results<Ok<Product>, NotFound> (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    return product is not null ? TypedResults.Ok(product) : TypedResults.NotFound();
});
```

### Built-in Validation (.NET 10)

```csharp
builder.Services.AddValidation();

public record CreateProductRequest(
    [Required, StringLength(100)] string Name,
    [Range(0.01, 999999.99)] decimal Price
);
```

### Route Groups

```csharp
var productsGroup = app.MapGroup("/api/products")
    .WithTags("Products")
    .AddEndpointFilter<LoggingFilter>();
```

## License

MIT
