using FluentValidation;
using FluentValidation.Api.Filters;
using FluentValidation.Api.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// Manual validation approach
app.MapPost("/register", async (UserRegistrationRequest request, IValidator<UserRegistrationRequest> validator) =>
{
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }
    // perform actual service call to register the user
    return Results.Accepted();
});

// Endpoint filter approach — auto-validation
app.MapPost("/register-with-filter", (UserRegistrationRequest request) =>
{
    // validation already handled by the filter
    return Results.Accepted();
})
.AddEndpointFilter<ValidationFilter<UserRegistrationRequest>>();

app.Run();
