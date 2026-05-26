using Microsoft.AspNetCore.Cors;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// ── CORS policies loaded from configuration ────────────────────────────────
// Per-environment lists live in appsettings.{Environment}.json so localhost
// origins never leak into production.
builder.Services.AddCors(options =>
{
    foreach (var policySection in builder.Configuration.GetSection("Cors").GetChildren())
    {
        var policyName = policySection.Key;
        var settings = policySection.Get<CorsPolicySettings>() ?? new CorsPolicySettings();

        options.AddPolicy(policyName, policy =>
        {
            if (settings.Origins.Length > 0)
            {
                policy.WithOrigins(settings.Origins);
            }

            if (settings.Methods.Length > 0)
            {
                policy.WithMethods(settings.Methods);
            }
            else
            {
                policy.AllowAnyMethod();
            }

            if (settings.Headers.Length > 0)
            {
                policy.WithHeaders(settings.Headers);
            }
            else
            {
                policy.AllowAnyHeader();
            }

            if (settings.ExposedHeaders.Length > 0)
            {
                policy.WithExposedHeaders(settings.ExposedHeaders);
            }

            if (settings.AllowCredentials)
            {
                policy.AllowCredentials();
            }

            policy.SetPreflightMaxAge(TimeSpan.FromSeconds(settings.PreflightMaxAgeSeconds));
        });
    }
});

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// ── Middleware order ───────────────────────────────────────────────────────
// UseRouting → UseCors → UseAuthentication → UseAuthorization.
// UseCors must run before authentication so the unauthenticated OPTIONS
// preflight does not get rejected with a 401 before CORS headers are written.
app.UseRouting();
app.UseCors("AcmeFrontend");
// app.UseAuthentication();   // wired up when auth is added
// app.UseAuthorization();    // wired up when auth is added

// ── Endpoints ──────────────────────────────────────────────────────────────

// Product listing - uses the global AcmeFrontend policy applied above.
// Also returns X-Total-Count so we exercise WithExposedHeaders.
app.MapGet("/api/products", (HttpContext http) =>
{
    var products = new[]
    {
        new { Id = 1, Name = "Course A", Price = 49 },
        new { Id = 2, Name = "Course B", Price = 99 },
        new { Id = 3, Name = "Course C", Price = 149 }
    };

    http.Response.Headers["X-Total-Count"] = products.Length.ToString();
    return Results.Ok(products);
})
.WithTags("Products")
.WithSummary("List products (global AcmeFrontend CORS policy applies)");

// Create a product - shows preflight will fire for POST + Content-Type: application/json.
app.MapPost("/api/products", (Product input) =>
{
    var created = input with { Id = Random.Shared.Next(1000, 9999) };
    return Results.Created($"/api/products/{created.Id}", created);
})
.WithTags("Products")
.WithSummary("Create a product (triggers a preflight)");

// Public Stripe-style webhook receiver - locked to its own origin via RequireCors.
// This endpoint ignores the global AcmeFrontend policy.
app.MapPost("/webhooks/stripe", (StripeEvent payload, ILogger<Program> logger) =>
{
    logger.LogInformation("Received Stripe event {EventType} {EventId}", payload.Type, payload.Id);
    return Results.Accepted();
})
.RequireCors("PublicWebhooks")
.WithTags("Webhooks")
.WithSummary("Webhook receiver (endpoint-specific CORS policy)");

// Diagnostic endpoint that echoes the request's Origin header back so you can
// confirm exactly which origin the browser sent on cross-origin calls.
app.MapGet("/api/echo-origin", (HttpContext http) =>
{
    var origin = http.Request.Headers.Origin.ToString();
    return Results.Ok(new
    {
        Origin = string.IsNullOrEmpty(origin) ? "(no Origin header - same-origin or non-browser client)" : origin,
        Path = http.Request.Path.Value,
        Method = http.Request.Method
    });
})
.WithTags("Diagnostics")
.WithSummary("Echo back the Origin header for debugging CORS");

// Liveness endpoint - opted out of CORS entirely with [DisableCors].
// Useful for load balancers and monitoring agents that should never carry an
// Origin header at all.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithMetadata(new DisableCorsAttribute())
    .WithTags("Health")
    .WithSummary("Liveness probe (CORS disabled)");

app.Run();

public record Product(int Id, string Name, decimal Price);

public record StripeEvent(string Id, string Type, object? Data);

public sealed class CorsPolicySettings
{
    public string[] Origins { get; set; } = [];
    public string[] Methods { get; set; } = [];
    public string[] Headers { get; set; } = [];
    public string[] ExposedHeaders { get; set; } = [];
    public bool AllowCredentials { get; set; }
    public int PreflightMaxAgeSeconds { get; set; } = 600;
}
