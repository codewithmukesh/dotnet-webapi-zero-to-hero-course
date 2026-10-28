# Clean Architecture vs Vertical Slice Architecture

This folder contains three .NET 10 Web API solutions that implement the same Product API using different architectural patterns:

1. **Clean Architecture** - Traditional layered approach with Domain, Application, Infrastructure, and API layers
2. **Vertical Slice Architecture** - Feature-based approach where each feature is self-contained in a single file
3. **Hybrid** (recommended) - A Domain project for business rules, an Infrastructure project for EF Core, and feature folders in the API project. No Application project, no mediator, with architecture tests.

The Clean Architecture and Vertical Slice projects use:
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

## Running the Hybrid Project

```bash
cd Hybrid
dotnet build Hybrid.slnx
dotnet run --project src/Hybrid.Api
```

In Development, the app creates the `HybridProductDb` database on startup (demo only, use migrations in real apps).

Run the architecture tests (xUnit v3 test projects are executables, so use `dotnet run`, not `dotnet test`):

```bash
dotnet run --project tests/Hybrid.ArchitectureTests
```

The Hybrid project uses FluentValidation 12.1.1 with an endpoint filter for input validation, `IExceptionHandler` + Problem Details for domain rule violations, EF Core 10.0.12, Npgsql 10.0.3, Scalar 2.17.4, and NetArchTest 1.3.2. It needs PostgreSQL on `localhost:5432` (user and password `postgres`).

`Product` has a concurrency token (`Version`, mapped to the PostgreSQL `xmin` column), so a conflicting update returns 409 Conflict and the client should retry.

One thing is left out on purpose to keep the sample small: domain rules throw a `DomainException`. The Result pattern is the better choice in a real app.

## API Endpoints (Clean Architecture and Vertical Slice)

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
