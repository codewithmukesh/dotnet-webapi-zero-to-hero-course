# FluentValidation in ASP.NET Core

Clean, maintainable request validation using FluentValidation — a powerful alternative to Data Annotations.

## Resources

- **Article**: [FluentValidation in ASP.NET Core - Super Powerful Validations](https://codewithmukesh.com/blog/fluentvalidation-in-aspnet-core/)
- **Course**: [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

## What You'll Learn

- Replace Data Annotations with FluentValidation for cleaner separation of concerns
- Build reusable validator classes with fluent rule syntax
- Wire up validation filters to automatically validate incoming requests
- Return structured validation error responses

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Quick Start

```bash
cd FluentValidation.Api
dotnet run
```

Open `http://localhost:5000/scalar/v1` for API documentation.

## Project Structure

```
FluentValidation.Api/
├── Program.cs                          # Endpoint definitions and DI setup
├── Models/
│   └── UserRegistrationRequest.cs      # Request DTO
├── Validators/
│   └── UserRegistrationValidator.cs    # Fluent validation rules
├── Filters/
│   └── ValidationFilter.cs            # Endpoint filter for automatic validation
└── FluentValidation.Api.csproj
```

## Key Technologies

- FluentValidation 12.x
- FluentValidation.DependencyInjectionExtensions
- Scalar (OpenAPI docs)

## License

MIT
