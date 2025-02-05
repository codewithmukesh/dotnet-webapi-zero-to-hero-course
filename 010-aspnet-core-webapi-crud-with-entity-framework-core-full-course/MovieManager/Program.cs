using Microsoft.EntityFrameworkCore;
using MovieManager.Endpoints;
using MovieManager.Persistence;
using MovieManager.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddDbContext<MovieDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});
builder.Services.AddTransient<IMovieService, MovieService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

await using (var serviceScope = app.Services.CreateAsyncScope())
await using (var dbContext = serviceScope.ServiceProvider.GetRequiredService<MovieDbContext>())
{
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!")
    .Produces(200, typeof(string));

app.MapMovieEndpoints();

app.Run();
