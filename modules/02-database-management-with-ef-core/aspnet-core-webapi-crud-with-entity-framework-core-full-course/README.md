# ASP.NET Core 10 Web API CRUD with Entity Framework Core

This project demonstrates building a production-ready ASP.NET Core 10 Web API with Entity Framework Core, PostgreSQL, and best practices including Domain-Driven Design (DDD), DTOs, and Minimal APIs.

## 🚀 Features

- **.NET 10** - Latest LTS version with improved performance
- **Entity Framework Core 10** - Code First approach with migrations
- **PostgreSQL** - Running in Docker container
- **Domain-Driven Design** - Clean domain entities with validation
- **Minimal APIs** - Lightweight, performant endpoints
- **Scalar UI** - Modern OpenAPI documentation interface
- **DTOs** - Proper data transfer objects for API contracts
- **Repository Pattern** - Clean separation of concerns

## 📋 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Visual Studio 2026](https://visualstudio.microsoft.com/downloads/) or VS Code with C# Dev Kit

## 🛠️ Setup Instructions

### 1. Start PostgreSQL with Docker

```bash
docker compose up -d
```

This will start a PostgreSQL 17 (Alpine) database on `localhost:5432` with:
- Database: `dotnetHero`
- Username: `admin`
- Password: `secret`

### 2. Restore NuGet Packages

```bash
dotnet restore
```

### 3. Run Migrations

```bash
dotnet ef migrations add InitialCreate --project MovieApi.Api
dotnet ef database update --project MovieApi.Api
```

### 4. Run the Application

```bash
dotnet run --project MovieApi.Api
```

The API will be available at:
- HTTPS: `https://localhost:7157`
- HTTP: `http://localhost:5131`
- Scalar UI: `https://localhost:7157/scalar/v1`

## 🎯 API Endpoints

### Create a Movie
```http
POST /api/movies
Content-Type: application/json

{
  "title": "The Matrix",
  "genre": "Sci-Fi",
  "releaseDate": "1999-03-31T00:00:00Z",
  "rating": 8.7
}
```

### Get All Movies
```http
GET /api/movies
```

### Get Movie by ID
```http
GET /api/movies/{id}
```

### Update a Movie
```http
PUT /api/movies/{id}
Content-Type: application/json

{
  "title": "The Matrix Reloaded",
  "genre": "Sci-Fi",
  "releaseDate": "2003-05-15T00:00:00Z",
  "rating": 7.2
}
```

### Delete a Movie
```http
DELETE /api/movies/{id}
```

## 📁 Project Structure

```
MovieApi.Api/
├── Models/                  # Domain entities
│   ├── EntityBase.cs
│   └── Movie.cs
├── DTOs/                    # Data transfer objects
│   ├── CreateMovieDto.cs
│   ├── UpdateMovieDto.cs
│   └── MovieDto.cs
├── Persistence/             # Database context and configurations
│   ├── MovieDbContext.cs
│   └── Configurations/
│       └── MovieConfiguration.cs
├── Services/                # Business logic
│   ├── IMovieService.cs
│   └── MovieService.cs
├── Endpoints/               # API endpoints
│   └── MovieEndpoints.cs
└── Program.cs               # Application entry point
```

## 🔧 Connection String

The default connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dotnetHero;Username=admin;Password=secret;"
  }
}
```

**⚠️ For production**: Use environment variables, Azure Key Vault, or AWS Secrets Manager instead of hardcoding credentials.

## 🧪 Testing

You can test the API using:
- **Scalar UI** - Navigate to `/scalar/v1` when running locally
- **Postman** - Import the endpoints manually
- **curl** - Use command line
- **HTTPie** - For a better CLI experience

## 📚 Learn More

This code accompanies the article:
- [ASP.NET Core 10 Web API CRUD with Entity Framework Core - Complete Tutorial](https://codewithmukesh.com/blog/aspnet-core-webapi-crud-with-entity-framework-core-full-course)

## 🤝 Contributing

This is a sample project for educational purposes. Feel free to use it as a starting point for your own projects.

## 📄 License

This project is provided as-is for educational purposes.

## 👨‍💻 Author

**Mukesh Murugan**
- Website: [codewithmukesh.com](https://codewithmukesh.com)
- Twitter: [@iammukeshm](https://twitter.com/iammukeshm)
- LinkedIn: [Mukesh Murugan](https://www.linkedin.com/in/iammukeshm/)
