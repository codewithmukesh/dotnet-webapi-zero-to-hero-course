using HealthChecks.Api.Data;
using HealthChecks.Api.HealthChecks;
using HealthChecks.Api.Models;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddHttpClient("payments", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PaymentGateway:BaseUrl"]!);
});

// Every dependency check is tagged "ready" so the readiness endpoint can filter
// them in while the liveness endpoint runs none of them.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: "postgres", tags: ["ready"])
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis")!,
        name: "redis",
        tags: ["ready"])
    .AddUrlGroup(
        new Uri("https://codewithmukesh.com"),
        name: "blog",
        tags: ["ready"])
    .AddCheck<PaymentGatewayHealthCheck>(name: "payment-gateway", tags: ["ready"]);

builder.Services
    .AddHealthChecksUI(options =>
    {
        options.AddHealthCheckEndpoint("API", "/health/ready");
        options.SetEvaluationTimeInSeconds(15);
    })
    .AddInMemoryStorage();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// Detailed JSON report: which check failed, how long each took, any error.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteDetailedJsonResponse
});

// Readiness: run only dependency checks (tagged "ready"). UI-compatible JSON.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Liveness: run NO checks. A 200 just means the process can serve a request.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecksUI(options => options.UIPath = "/health-ui");

var products = app.MapGroup("/products").WithTags("Products");

products.MapGet("/", async (AppDbContext db, CancellationToken cancellationToken) =>
{
    var result = await db.Products.AsNoTracking().ToListAsync(cancellationToken);
    return TypedResults.Ok(result);
})
.WithName("GetAllProducts")
.WithSummary("Get all products");

app.Run();

static Task WriteDetailedJsonResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";

    var response = new
    {
        status = report.Status.ToString(),
        totalDurationMs = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            description = entry.Value.Description,
            durationMs = entry.Value.Duration.TotalMilliseconds,
            error = entry.Value.Exception?.Message
        })
    };

    return context.Response.WriteAsJsonAsync(response);
}
