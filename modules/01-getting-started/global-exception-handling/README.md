# Global Exception Handling in ASP.NET Core - .NET 10

Production-ready global exception handling using `IExceptionHandler`, `IProblemDetailsService`, and RFC 9457-compliant `ProblemDetails` responses.

## What You'll Learn

- Implement `IExceptionHandler` (the modern .NET 8+ approach)
- Create custom exception classes with embedded HTTP status codes
- Use `IProblemDetailsService` for standardized error responses
- Chain multiple exception handlers for different exception types
- Secure error messages (hide internals in production)
- Use .NET 10's `SuppressDiagnosticsCallback` to control logging

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2026 / VS Code / Rider

## Quick Start

```bash
cd GlobalExceptionHandling.Api
dotnet run
```

Open `http://localhost:5000/scalar/v1` to explore the API.

## Project Structure

```
GlobalExceptionHandling.Api/
├── Exceptions/
│   ├── AppException.cs           # Base exception with HttpStatusCode
│   ├── NotFoundException.cs      # 404 Not Found
│   ├── BadRequestException.cs    # 400 Bad Request
│   ├── ConflictException.cs      # 409 Conflict
│   └── ValidationException.cs    # 400 with field-level errors
├── Handlers/
│   ├── GlobalExceptionHandler.cs      # Main handler (catches all)
│   ├── NotFoundExceptionHandler.cs    # Specialized 404 handler
│   └── ValidationExceptionHandler.cs  # Specialized validation handler
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

## Test Endpoints

| Endpoint | Method | Exception Type | Status |
|----------|--------|----------------|--------|
| `/` | GET | None | 200 |
| `/products/{id}` | GET | `NotFoundException` | 404 |
| `/products` | POST | `BadRequestException` | 400 |
| `/products/validate` | POST | `ValidationException` | 400 |
| `/products/duplicate` | POST | `ConflictException` | 409 |
| `/error` | GET | `InvalidOperationException` | 500 |
| `/error/null` | GET | `NullReferenceException` | 500 |
| `/error/argument` | GET | `ArgumentNullException` | 400 |

## Try It Out

```bash
# 404 - Not Found
curl http://localhost:5000/products/550e8400-e29b-41d4-a716-446655440000

# 400 - Bad Request
curl -X POST http://localhost:5000/products \
  -H "Content-Type: application/json" \
  -d '{"name": "", "price": 0}'

# 400 - Validation errors
curl -X POST http://localhost:5000/products/validate \
  -H "Content-Type: application/json" \
  -d '{"name": "", "price": -5}'

# 409 - Conflict
curl -X POST http://localhost:5000/products/duplicate \
  -H "Content-Type: application/json" \
  -d '{"name": "Widget", "price": 10}'

# 500 - Unexpected error
curl http://localhost:5000/error
```

## Sample Response (404 Not Found)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Product with identifier '550e8400-e29b-41d4-a716-446655440000' was not found.",
  "status": 404,
  "instance": "GET /products/550e8400-e29b-41d4-a716-446655440000",
  "traceId": "0HN123ABC:00000001",
  "timestamp": "2026-01-24T10:30:00.000Z"
}
```

## Handler Chaining

By default, only `GlobalExceptionHandler` is registered. To see handler chaining in action, uncomment the specialized handlers in `Program.cs`:

```csharp
// Handlers execute in registration order
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();      // Handles NotFoundException
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();    // Handles ValidationException
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();        // Fallback for everything else
```

Each handler returns:
- `true` → "I handled it, stop here"
- `false` → "Not my type, try the next handler"

## Key Implementation Details

### Custom Exceptions Carry Status Codes

```csharp
public abstract class AppException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

// Usage
throw new NotFoundException("Product", productId);  // Returns 404
throw new BadRequestException("Invalid input");     // Returns 400
```

### GlobalExceptionHandler

- Logs with `TraceIdentifier` for request correlation
- Maps exception types to HTTP status codes
- Uses `IProblemDetailsService` for RFC 9457 responses
- Hides internal error details in production

### Safe Error Messages

```csharp
// Development: Show all error messages
// Production: Only show messages from AppException types
private static string? GetSafeErrorMessage(Exception exception, HttpContext context)
{
    var env = context.RequestServices.GetRequiredService<IHostEnvironment>();
    if (env.IsDevelopment()) return exception.Message;
    return exception is AppException ? exception.Message : null;
}
```

## Related Resources

- **Full Tutorial**: [Global Exception Handling in ASP.NET Core](https://codewithmukesh.com/blog/global-exception-handling-in-aspnet-core/)
- **Course**: [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

## License

MIT
