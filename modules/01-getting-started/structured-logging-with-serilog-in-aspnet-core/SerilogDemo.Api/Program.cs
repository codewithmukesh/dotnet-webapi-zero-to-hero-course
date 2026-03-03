using Scalar.AspNetCore;
using Serilog;
using SerilogDemo.Api.Middleware;
using SerilogDemo.Api.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting server.");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services));

    builder.Services.AddOpenApi();
    builder.Services.AddTransient<IWeatherService, WeatherService>();

    var app = builder.Build();

    app.MapOpenApi();
    app.MapScalarApiReference();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        };

        options.GetLevel = (httpContext, elapsed, ex) =>
        {
            if (httpContext.Request.Path.StartsWithSegments("/health"))
                return Serilog.Events.LogEventLevel.Verbose;

            return elapsed > 500
                ? Serilog.Events.LogEventLevel.Warning
                : Serilog.Events.LogEventLevel.Information;
        };
    });

    app.UseHttpsRedirection();

    app.MapGet("/", () => "Hello from Serilog!");

    app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

    app.MapGet("/weather", (IWeatherService svc) => svc.GetForecast());

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Server terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
