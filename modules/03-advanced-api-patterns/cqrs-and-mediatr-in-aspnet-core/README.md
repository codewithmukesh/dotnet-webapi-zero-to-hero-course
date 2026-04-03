# CQRS and MediatR in ASP.NET Core

Implement the CQRS pattern with MediatR 14.1 in .NET 10 - full CRUD API with commands, queries, pipeline behaviors, and notifications.

## Resources

- **Article**: [CQRS and MediatR in ASP.NET Core - Complete Guide](https://codewithmukesh.com/blog/cqrs-and-mediatr-in-aspnet-core/)
- **Course**: [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

## What You'll Learn

- Separate read and write operations using the CQRS pattern
- Implement commands (Create, Update, Delete) and queries (Get, List) with MediatR
- Use `ISender` for request/response and `IMediator` for notifications
- Publish notifications to multiple decoupled handlers
- Organize features using Vertical Slice Architecture folder structure
- Use records as immutable DTOs and primary constructors for handlers

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Quick Start

```bash
cd CqrsMediatr.Api
dotnet run
```

Open `http://localhost:5000/scalar/v1` for API documentation.

## Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/products` | List all products |
| GET | `/products/{id}` | Get product by ID |
| POST | `/products` | Create a new product (publishes notification) |
| PUT | `/products/{id}` | Update an existing product |
| DELETE | `/products/{id}` | Delete a product |

## Project Structure

```
CqrsMediatr.Api/
├── Program.cs                          # Endpoints, DI, and middleware
├── Domain/
│   └── Product.cs                      # Product entity with encapsulated mutation
├── Persistence/
│   └── AppDbContext.cs                 # EF Core InMemory DbContext with seed data
├── Features/Products/
│   ├── DTOs/
│   │   └── ProductDto.cs              # Immutable record DTO
│   ├── Commands/
│   │   ├── Create/                    # CreateProductCommand + Handler
│   │   ├── Update/                    # UpdateProductCommand + Handler
│   │   └── Delete/                    # DeleteProductCommand + Handler
│   ├── Queries/
│   │   ├── Get/                       # GetProductQuery + Handler
│   │   └── List/                      # ListProductsQuery + Handler
│   └── Notifications/
│       ├── ProductCreatedNotification.cs
│       ├── StockAssignedHandler.cs    # Reacts to product creation
│       └── AuditLogHandler.cs          # Audit log notification handler
└── CqrsMediatr.Api.csproj
```

## Key Packages

| Package | Version | Purpose |
|---------|---------|---------|
| MediatR | 14.1.0 | In-process mediator for CQRS |
| Microsoft.EntityFrameworkCore | 10.0.0 | ORM |
| Microsoft.EntityFrameworkCore.InMemory | 10.0.0 | InMemory database for demo |
| Scalar.AspNetCore | 2.13.18 | API documentation UI |

## License

MIT
