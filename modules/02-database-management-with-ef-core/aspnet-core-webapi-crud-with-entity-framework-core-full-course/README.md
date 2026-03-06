# ASP.NET Core Web API CRUD with Entity Framework Core

Build a complete Movie API with full CRUD operations, EF Core code-first migrations, Domain-Driven Design entities, DTOs, and PostgreSQL.

## Resources

- **Article**: [ASP.NET Core 10 Web API CRUD with Entity Framework Core - Complete Tutorial](https://codewithmukesh.com/blog/aspnet-core-webapi-crud-with-entity-framework-core-full-course/)
- **Course**: [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

## What You'll Learn

- Set up EF Core with PostgreSQL (code-first approach)
- Design domain entities with DDD principles
- Create DTOs for clean API contracts
- Implement a service layer for business logic
- Run database migrations
- Build Minimal API endpoints for all CRUD operations

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Quick Start

```bash
# Start PostgreSQL
docker compose up -d

# Run migrations
dotnet ef migrations add InitialCreate --project MovieApi.Api
dotnet ef database update --project MovieApi.Api

# Start the API
dotnet run --project MovieApi.Api
```

- HTTPS: `https://localhost:7157`
- HTTP: `http://localhost:5131`
- Scalar UI: `https://localhost:7157/scalar/v1`

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/movies` | Create a movie |
| GET | `/api/movies` | Get all movies |
| GET | `/api/movies/{id}` | Get movie by ID |
| PUT | `/api/movies/{id}` | Update a movie |
| DELETE | `/api/movies/{id}` | Delete a movie |

## Project Structure

```
MovieApi.Api/
├── Program.cs                          # App entry point and DI setup
├── Models/
│   ├── EntityBase.cs                   # Base entity with Id and timestamps
│   └── Movie.cs                        # Movie domain entity
├── DTOs/
│   ├── CreateMovieDto.cs
│   ├── UpdateMovieDto.cs
│   └── MovieDto.cs
├── Persistence/
│   ├── MovieDbContext.cs               # EF Core DbContext
│   └── Configurations/
│       └── MovieConfiguration.cs       # Fluent entity configuration
├── Services/
│   ├── IMovieService.cs
│   └── MovieService.cs                 # Business logic layer
├── Endpoints/
│   └── MovieEndpoints.cs               # Minimal API route definitions
└── Migrations/                         # Auto-generated EF Core migrations
```

## Database Configuration

| Setting | Value |
|---------|-------|
| Host | `localhost` |
| Port | `5432` |
| Database | `dotnetHero` |
| Username | `admin` |
| Password | `secret` |

> For production, use environment variables or a secrets manager for connection strings.

## License

MIT
