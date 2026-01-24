using GlobalExceptionHandling.Api.Exceptions;
using GlobalExceptionHandling.Api.Handlers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure ProblemDetails with global customization
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
        ctx.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        ctx.ProblemDetails.Instance = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
    };
});

// Register exception handlers in order (first match wins)
// Uncomment the specialized handlers to see handler chaining in action
// builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
// builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();

var app = builder.Build();

// Add exception handling middleware early in the pipeline
app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference();

// Health check endpoint
app.MapGet("/", () => "Global Exception Handling Demo - .NET 10")
    .WithTags("Health")
    .WithName("HealthCheck")
    .WithSummary("Health check endpoint");

// Test endpoints for different exception types
app.MapGet("/products/{id:guid}", (Guid id) =>
{
    // Simulate not found scenario
    throw new NotFoundException("Product", id);
})
.WithTags("Products")
.WithName("GetProduct")
.WithSummary("Get a product by ID (always throws NotFoundException for demo)");

app.MapPost("/products", (ProductRequest request) =>
{
    // Validate request
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        throw new BadRequestException("Product name is required");
    }

    if (request.Price <= 0)
    {
        throw new ValidationException("Price", "Price must be greater than zero");
    }

    return Results.Created($"/products/{Guid.NewGuid()}", request);
})
.WithTags("Products")
.WithName("CreateProduct")
.WithSummary("Create a new product (validates input)");

app.MapPost("/products/validate", (ProductRequest request) =>
{
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(request.Name))
    {
        errors["Name"] = ["Product name is required"];
    }

    if (request.Price <= 0)
    {
        errors["Price"] = ["Price must be greater than zero"];
    }

    if (request.Name?.Length > 100)
    {
        errors["Name"] = [.. errors.GetValueOrDefault("Name", []), "Name cannot exceed 100 characters"];
    }

    if (errors.Count > 0)
    {
        throw new ValidationException(errors);
    }

    return Results.Ok(new { Message = "Validation passed", Product = request });
})
.WithTags("Products")
.WithName("ValidateProduct")
.WithSummary("Validate a product (demonstrates ValidationException with multiple errors)");

app.MapPost("/products/duplicate", (ProductRequest request) =>
{
    // Simulate conflict scenario
    throw new ConflictException($"A product with name '{request.Name}' already exists");
})
.WithTags("Products")
.WithName("CreateDuplicateProduct")
.WithSummary("Create a duplicate product (always throws ConflictException for demo)");

app.MapGet("/error", () =>
{
    // Simulate unexpected error
    throw new InvalidOperationException("Something went terribly wrong!");
})
.WithTags("Errors")
.WithName("TriggerError")
.WithSummary("Trigger an unexpected error (throws InvalidOperationException)");

app.MapGet("/error/null", () =>
{
    // Simulate null reference
    string? value = null;
    return value!.Length;
})
.WithTags("Errors")
.WithName("TriggerNullReference")
.WithSummary("Trigger a null reference error");

app.MapGet("/error/argument", (int? value) =>
{
    if (value is null)
    {
        throw new ArgumentNullException(nameof(value), "Value cannot be null");
    }
    return Results.Ok(value);
})
.WithTags("Errors")
.WithName("TriggerArgumentNull")
.WithSummary("Trigger an ArgumentNullException when value is not provided");

app.Run();

// Request/Response models
public record ProductRequest(string Name, decimal Price);
