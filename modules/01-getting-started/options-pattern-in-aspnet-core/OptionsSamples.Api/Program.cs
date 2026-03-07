using Microsoft.Extensions.Options;
using OptionsSamples.Api.Options;
using OptionsSamples.Api.Validation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// IOptions<WeatherOptions> - basic binding with data annotation validation
builder.Services.AddOptions<WeatherOptions>()
    .BindConfiguration(WeatherOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// IValidateOptions<T> - custom validation
builder.Services.AddSingleton<IValidateOptions<WeatherOptions>, WeatherOptionsValidator>();

// Named options for notification providers
builder.Services.AddOptions<NotificationOptions>()
    .BindConfiguration(NotificationOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// PostConfigure - override options after binding
builder.Services.PostConfigure<WeatherOptions>(options =>
{
    if (string.IsNullOrWhiteSpace(options.Summary))
    {
        options.Summary = options.Temperature switch
        {
            <= 10 => "Cold",
            <= 25 => "Warm",
            _ => "Hot"
        };
    }
});

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();

// Minimal API examples
app.MapGet("/api/weather/minimal", (IOptions<WeatherOptions> options) =>
{
    return Results.Ok(options.Value);
})
.WithName("GetWeatherMinimal")
.WithTags("Weather");

app.MapGet("/api/weather/minimal/snapshot", (IOptionsSnapshot<WeatherOptions> options) =>
{
    return Results.Ok(options.Value);
})
.WithName("GetWeatherSnapshot")
.WithTags("Weather");

app.MapGet("/api/weather/minimal/monitor", (IOptionsMonitor<WeatherOptions> options) =>
{
    return Results.Ok(options.CurrentValue);
})
.WithName("GetWeatherMonitor")
.WithTags("Weather");

// Accessing options in Program.cs
var weatherOptions = builder.Configuration
    .GetSection(WeatherOptions.SectionName)
    .Get<WeatherOptions>();
Console.WriteLine($"Startup - City: {weatherOptions?.City}, Temp: {weatherOptions?.Temperature}");

app.Run();
