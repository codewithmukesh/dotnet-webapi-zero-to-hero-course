using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RedisRateLimiting.AspNetCore;
using Scalar.AspNetCore;
using StackExchange.Redis;

static string ResolveClient(HttpContext httpContext)
{
    if (!string.IsNullOrEmpty(httpContext.User.Identity?.Name))
    {
        return httpContext.User.Identity.Name;
    }

    var apiKey = httpContext.Request.Headers["X-API-Key"].ToString();
    if (!string.IsNullOrEmpty(apiKey))
    {
        return apiKey;
    }

    return httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Optional Redis connection - turned on via appsettings.json when running multi-instance.
var enableRedisBackplane = builder.Configuration.GetValue<bool>("RateLimiting:EnableRedisBackplane");
if (enableRedisBackplane)
{
    var redisConnection = builder.Configuration.GetValue<string>("RateLimiting:Redis") ?? "localhost:6379";
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
}

builder.Services.AddRateLimiter(options =>
{
    // Override the default 503 - the framework default is wrong for rate limiting.
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Production-grade rejection: ProblemDetails body + Retry-After header + structured log.
    options.OnRejected = async (context, cancellationToken) =>
    {
        var httpContext = context.HttpContext;
        var logger = httpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiting");

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            httpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
        }

        var problem = new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc6585#section-4",
            Title = "Too many requests",
            Status = StatusCodes.Status429TooManyRequests,
            Detail = "You have exceeded the rate limit for this endpoint. Slow down and retry after the Retry-After header value.",
            Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        logger.LogWarning(
            "Rate limit exceeded. Path: {Path} Client: {Client} TraceId: {TraceId}",
            httpContext.Request.Path,
            ResolveClient(httpContext),
            httpContext.TraceIdentifier);

        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
    };

    // ----------------- Named policies (one per algorithm) -----------------

    // Token Bucket - default for public APIs.
    options.AddTokenBucketLimiter("public-api", opt =>
    {
        opt.TokenLimit = 100;
        opt.TokensPerPeriod = 100;
        opt.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        opt.AutoReplenishment = true;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // Fixed Window - cheap, predictable, for trusted internal RPC.
    options.AddFixedWindowLimiter("internal-rpc", opt =>
    {
        opt.PermitLimit = 1000;
        opt.Window = TimeSpan.FromSeconds(60);
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // Sliding Window - for login / OTP / password reset where boundary bursts are dangerous.
    options.AddSlidingWindowLimiter("auth-endpoints", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6; // 10-second segments
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // Concurrency Limiter - caps in-flight work on expensive endpoints (uploads, AI inference).
    options.AddConcurrencyLimiter("file-upload", opt =>
    {
        opt.PermitLimit = 8;
        opt.QueueLimit = 16;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // ----------------- Redis-backed policy (multi-instance) -----------------

    if (enableRedisBackplane)
    {
        var connection = ConnectionMultiplexer.Connect(
            builder.Configuration.GetValue<string>("RateLimiting:Redis") ?? "localhost:6379");

        options.AddRedisTokenBucketLimiter("public-api-redis", opt =>
        {
            opt.ConnectionMultiplexerFactory = () => connection;
            opt.TokenLimit = 100;
            opt.TokensPerPeriod = 100;
            opt.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        });
    }

    // ----------------- Global limiter (multi-tenant tier resolution) -----------------

    var tiers = builder.Configuration.GetSection("Tiers").Get<Dictionary<string, string>>()
        ?? new Dictionary<string, string>();

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var apiKey = httpContext.Request.Headers["X-API-Key"].ToString();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            // Unauthenticated traffic shares a single tight bucket.
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: "anonymous",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
        }

        // Look up the tier - in production this comes from HybridCache, not a dictionary.
        if (!tiers.TryGetValue(apiKey, out var tierName))
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: "invalid-key",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
        }

        return tierName switch
        {
            "Free" => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: apiKey,
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 60,
                    TokensPerPeriod = 60,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    AutoReplenishment = true,
                    QueueLimit = 0
                }),

            "Pro" => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: apiKey,
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 600,
                    TokensPerPeriod = 600,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    AutoReplenishment = true,
                    QueueLimit = 0
                }),

            "Enterprise" => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: apiKey,
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 6000,
                    TokensPerPeriod = 6000,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    AutoReplenishment = true,
                    QueueLimit = 0
                }),

            _ => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: "invalid-key",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                })
        };
    });
});

var app = builder.Build();

app.UseRouting();
app.UseRateLimiter();

app.MapOpenApi();
app.MapScalarApiReference();

// Public API - Token Bucket
app.MapGet("/api/pricing", () => Results.Ok(new { tier = "pro", price = 49 }))
   .RequireRateLimiting("public-api");

// Internal RPC - Fixed Window
app.MapGet("/api/internal/health", () => Results.Ok(new { ok = true }))
   .RequireRateLimiting("internal-rpc");

// Auth endpoint - Sliding Window
app.MapPost("/api/login", (LoginRequest req) =>
    Results.Ok(new { token = "demo-token" }))
   .RequireRateLimiting("auth-endpoints");

// File upload - Concurrency Limiter
app.MapPost("/api/upload", async (HttpRequest request) =>
{
    await Task.Delay(500); // simulate work
    return Results.Ok(new { uploaded = true });
})
.RequireRateLimiting("file-upload");

// Health check - always exempt from rate limiting.
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
   .DisableRateLimiting();

app.Run();

public sealed record LoginRequest(string Email, string Password);
