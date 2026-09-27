using CartesianExplosion.Api.Data;
using CartesianExplosion.Api.Queries;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Default Npgsql pool settings on purpose (Max Pool Size 100, Timeout 15s, Command Timeout 30s):
// the video quotes these defaults.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// The health check opens a real connection from the same pool, so it shows pool starvation.
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (app.Configuration["Seed:Size"] is { } seedSize)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await Seeder.ResetAsync(db, SeedSize.Parse(seedSize), CancellationToken.None);
}

app.MapGet("/departments/single", async (AppDbContext db, CancellationToken ct, bool documents = false) =>
    await DepartmentQueries.Single(db, documents).ToListAsync(ct));

app.MapGet("/departments/split", async (AppDbContext db, CancellationToken ct, bool documents = false) =>
    await DepartmentQueries.Split(db, documents).ToListAsync(ct));

app.MapHealthChecks("/health");

app.Run();

public partial class Program;
