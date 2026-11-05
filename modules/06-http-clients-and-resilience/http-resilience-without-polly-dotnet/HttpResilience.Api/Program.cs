using HttpResilience.Handlers;
using Microsoft.Extensions.Http.Resilience;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var catalogUrl = new Uri(builder.Configuration["CatalogApi:BaseUrl"]!);

// Option 1: Microsoft.Extensions.Http.Resilience (Polly arrives as a transitive dependency).
builder.Services.AddHttpClient("catalog-microsoft", c => c.BaseAddress = catalogUrl)
    .AddStandardResilienceHandler(o => o.Retry.DisableForUnsafeHttpMethods());

// Option 2: Kevlar (MIT, no Polly anywhere in the dependency graph).
builder.Services.AddHttpClient("catalog-kevlar", c => c.BaseAddress = catalogUrl)
    .AddStandardShield();

// Option 3: hand-rolled DelegatingHandlers (no third-party package at all).
builder.Services.AddHttpClient("catalog-handrolled", c => c.BaseAddress = catalogUrl)
    .AddHandRolledResilience();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// Calls the flaky catalog API through the chosen client. Try failures=2 to see retries succeed.
app.MapGet("/products/{option}", async (string option, int failures, IHttpClientFactory factory, CancellationToken ct) =>
{
    var client = factory.CreateClient($"catalog-{option}");
    var response = await client.GetAsync($"/flaky/{Guid.NewGuid():N}?failures={failures}", ct);
    return Results.Ok(new { option, status = (int)response.StatusCode });
});

app.Run();
