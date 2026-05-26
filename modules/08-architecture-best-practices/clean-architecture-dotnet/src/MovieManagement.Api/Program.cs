using MovieManagement.Api.Endpoints;
using MovieManagement.Api.Infrastructure;
using MovieManagement.Application;
using MovieManagement.Infrastructure;
using MovieManagement.Infrastructure.Persistence;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Aspire: OpenTelemetry, health checks, service discovery, resilient HTTP.
builder.AddServiceDefaults();

// Aspire reads the "moviesdb" connection string it injected and registers
// ApplicationDbContext with the Npgsql provider - in a single call.
builder.AddNpgsqlDbContext<ApplicationDbContext>("moviesdb");

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddOpenApi();

// Accept and return enums as their string names ("Action") instead of numbers.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Turn domain rule violations into 400 ProblemDetails responses.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

var app = builder.Build();

// DEMO ONLY: create and migrate the database, then seed sample data, so the app
// runs with a single command. Never auto-migrate or auto-seed in production -
// apply migrations from your deployment pipeline instead.
if (app.Environment.IsDevelopment())
{
    await app.Services.InitializeDatabaseAsync();
}

app.UseExceptionHandler();

// Aspire health endpoints (/health and /alive).
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapMovieEndpoints();

app.Run();
