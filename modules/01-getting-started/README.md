# Module 01: Getting Started with ASP.NET Core Web API

Build a rock-solid foundation in .NET Web API development — from REST principles to structured logging and production-grade error handling.

Part of the **[.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)** course.

## Lessons

| # | Lesson | What You'll Learn | Article | Code |
|---|--------|-------------------|---------|------|
| 1 | REST API Best Practices | REST principles, resource naming, HTTP methods, status codes, API versioning | [Read](https://codewithmukesh.com/blog/restful-api-best-practices-for-dotnet-developers/) | [Code](./restful-api-best-practices-for-dotnet-developers/) |
| 2 | File-Based Apps in .NET 10 | Run C# from single files, `#:package` directives, CLI tools without `.csproj` | [Read](https://codewithmukesh.com/blog/file-based-apps-dotnet-10/) | [Code](./file-based-apps/) |
| 3 | Minimal API Endpoints | Route handlers, parameter binding, TypedResults, route groups, endpoint filters, built-in validation | [Read](https://codewithmukesh.com/blog/minimal-apis-aspnet-core/) | [Code](./minimal-apis-aspnet-core/) |
| 4 | Global Exception Handling | `IExceptionHandler`, custom exceptions, ProblemDetails (RFC 9457), handler chaining | [Read](https://codewithmukesh.com/blog/global-exception-handling-in-aspnet-core/) | [Code](./global-exception-handling/) |
| 5 | FluentValidation | Fluent validation rules, custom validators, validation filters, replacing Data Annotations | [Read](https://codewithmukesh.com/blog/fluentvalidation-in-aspnet-core/) | [Code](./fluentvalidation-in-aspnet-core/) |
| 6 | Structured Logging with Serilog | Serilog sinks (Console, File, Seq), enrichers, correlation IDs, structured log queries | [Read](https://codewithmukesh.com/blog/structured-logging-with-serilog-in-aspnet-core/) | [Code](./structured-logging-with-serilog-in-aspnet-core/) |
| 7 | Middleware & Request Pipeline | Coming Soon | — | — |
| 8 | Dependency Injection Deep Dive | Coming Soon | — | — |
| 9 | API Documentation with Scalar | Coming Soon | — | — |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022+ / VS Code / JetBrains Rider
- Docker Desktop (for the Serilog + Seq lesson)

## Quick Start

Each lesson is a self-contained project. Pick any lesson and run it:

```bash
# Example: Run the Minimal APIs project
cd minimal-apis-aspnet-core/MinimalApis.ProductCatalog.Api
dotnet run

# Open API docs at http://localhost:5000/scalar/v1
```

## Tech Stack

All projects in this module use:

- **.NET 10** with Minimal APIs
- **Scalar** for OpenAPI documentation
- **No database required** (except Serilog lesson which optionally uses Seq via Docker)
