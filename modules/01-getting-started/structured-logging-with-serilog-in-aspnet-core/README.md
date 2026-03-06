# Structured Logging with Serilog in ASP.NET Core

Production-ready structured logging setup with Serilog — multiple sinks, enrichers, correlation IDs, and Seq for log visualization.

## Resources

- **Article**: [Structured Logging with Serilog in ASP.NET Core](https://codewithmukesh.com/blog/structured-logging-with-serilog-in-aspnet-core/)
- **Course**: [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

## What You'll Learn

- Configure Serilog with multiple sinks (Console, File, Seq, OpenTelemetry)
- Add enrichers for environment, thread, and process context
- Implement correlation ID middleware for request tracing
- Query structured logs with Seq
- Configure log levels and filtering

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for Seq)

## Quick Start

```bash
# Start Seq for log visualization
docker-compose up -d

# Run the API
cd SerilogDemo.Api
dotnet run
```

- API: `http://localhost:5000/scalar/v1`
- Seq UI: `http://localhost:8081` (structured log viewer)
- Log files: `logs/log-*.json`

## Project Structure

```
SerilogDemo.Api/
├── Program.cs                              # Serilog configuration and setup
├── Models/
│   └── WeatherForecast.cs
├── Services/
│   ├── IWeatherService.cs
│   └── WeatherService.cs                   # Service with structured logging
├── Middleware/
│   └── CorrelationIdMiddleware.cs           # Adds correlation ID to every request
├── appsettings.json                         # Serilog sink configuration
├── docker-compose.yml                       # Seq container
└── SerilogDemo.Api.csproj
```

## Docker Services

| Service | Port | Purpose |
|---------|------|---------|
| Seq | 8081 (UI), 5341 (ingestion) | Structured log viewer and query engine |

## Key Technologies

- Serilog 5.x with AspNetCore integration
- Serilog.Sinks.Console, File, Seq, OpenTelemetry
- Serilog.Enrichers (Environment, Thread, Process)
- Serilog.Exceptions
- Scalar (OpenAPI docs)

## License

MIT
