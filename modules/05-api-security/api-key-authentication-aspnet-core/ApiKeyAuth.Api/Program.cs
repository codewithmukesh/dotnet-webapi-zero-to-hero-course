using ApiKeyAuth.Api.Authentication;
using ApiKeyAuth.Api.Data;
using ApiKeyAuth.Api.Endpoints;
using ApiKeyAuth.Api.Entities;
using ApiKeyAuth.Api.Validation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ApiKeyAuthDb"));

#pragma warning disable EXTEXP0018 // HybridCache is in preview
builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

builder.Services
    .AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationOptions.DefaultScheme, _ => { });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("keys:read", p => p.RequireClaim("scope", "keys:read"))
    .AddPolicy("keys:admin", p => p.RequireClaim("scope", "keys:admin"));

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<ApiKeySecuritySchemeTransformer>();
});

var app = builder.Build();

// Seed an admin key once on startup so the demo is usable out of the box.
// The plaintext is printed to the console; copy it before the process restarts.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var time = scope.ServiceProvider.GetRequiredService<TimeProvider>();

    if (!await db.ApiKeys.AnyAsync())
    {
        var plaintext = ApiKeyGenerator.Generate("sk_live_");
        db.ApiKeys.Add(new ApiKey
        {
            Prefix = plaintext[..12],
            KeyHash = ApiKeyHasher.Hash(plaintext),
            Name = "Bootstrap Admin Key",
            OwnerId = "bootstrap",
            Scopes = "keys:read,keys:admin",
            CreatedAt = time.GetUtcNow().UtcDateTime
        });
        await db.SaveChangesAsync();

        app.Logger.LogWarning(
            "Bootstrap admin API key (copy now, it is only shown once): {Key}",
            plaintext);
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapSecureEndpoints();
app.MapAdminEndpoints();

app.Run();

public partial class Program { }
