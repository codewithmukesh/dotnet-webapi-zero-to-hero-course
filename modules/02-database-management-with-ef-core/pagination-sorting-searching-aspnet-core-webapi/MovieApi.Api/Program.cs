using Microsoft.EntityFrameworkCore;
using MovieApi.Api.Endpoints;
using MovieApi.Api.Persistence;
using MovieApi.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();

// Configure DbContext with PostgreSQL
builder.Services.AddDbContext<MovieDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

// Register services
builder.Services.AddTransient<IMovieService, MovieService>();

var app = builder.Build();

// Ensure database is created (for development only)
await using (var serviceScope = app.Services.CreateAsyncScope())
await using (var dbContext = serviceScope.ServiceProvider.GetRequiredService<MovieDbContext>())
{
    await dbContext.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Map endpoints
app.MapGet("/", () => "Hello World!")
   .Produces(200, typeof(string));

app.MapMovieEndpoints();

app.Run();
