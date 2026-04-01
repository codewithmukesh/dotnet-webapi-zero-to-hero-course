# Clean Architecture vs Vertical Slice Architecture

This repository contains two complete .NET 10 Web API projects that implement the same Product CRUD API using different architectural patterns:

1. **Clean Architecture** - Traditional layered approach with Domain, Application, Infrastructure, and API layers
2. **Vertical Slice Architecture** - Feature-based approach where each feature is self-contained in a single file

Both projects use:
- .NET 10
- PostgreSQL with EF Core 10
- Mediator (source generator-based, by martinothamar)
- Scalar for API documentation
- Minimal APIs

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/download/) running on localhost
- A database named `ProductDb` (or update the connection string in `appsettings.json`)

## Running the Clean Architecture Project

```bash
cd CleanArchitecture
dotnet build CleanArchitecture.slnx
dotnet run --project CleanArchitecture.Api
```

Open your browser at `https://localhost:5001/scalar/v1` to explore the API.

## Running the Vertical Slice Project

```bash
cd VerticalSlice
dotnet build VerticalSlice.slnx
dotnet run --project VerticalSlice.Api
```

Open your browser at `https://localhost:5001/scalar/v1` to explore the API.

## API Endpoints

Both projects expose the same endpoints:

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/products` | Create a new product |
| GET | `/api/products` | Get all products |
| GET | `/api/products/{id}` | Get a product by ID |
| PUT | `/api/products/{id}` | Update a product |
| DELETE | `/api/products/{id}` | Delete a product |

## Key Differences

### Clean Architecture
- 4 projects with clear dependency rules (Domain -> Application -> Infrastructure -> API)
- Repository pattern abstracts data access
- Handlers depend on interfaces, not implementations
- More files, but each has a single responsibility

### Vertical Slice Architecture
- Single project with feature folders
- Each feature file contains its command/query, handler, and endpoint mapping
- Handlers use DbContext directly - no repository abstraction
- Fewer files, each feature is fully self-contained
