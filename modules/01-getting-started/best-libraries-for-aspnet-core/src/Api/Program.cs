using Api.Data;
using Api.Jobs;
using Api.Orders;
using Api.Products;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Quartz;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog - structured logging
builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// 2. FluentValidation - registers every validator in this assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// 3. Mapperly - the generated mapper has no state, so one instance is enough
builder.Services.AddSingleton<ProductMapper>();

// 4. EF Core - the connection string is named "shopdb" and comes from the Aspire AppHost
builder.Services.AddDbContext<AppDbContext>((services, options) =>
    options.UseNpgsql(services.GetRequiredService<IConfiguration>().GetConnectionString("shopdb")));

// 5. OpenAPI document (built into .NET) - Scalar is mapped further down
builder.Services.AddOpenApi();

// 6. HybridCache - Redis becomes the second level when a "cache" connection string exists
builder.Services.AddHybridCache();

var redis = builder.Configuration.GetConnectionString("cache");
if (!string.IsNullOrWhiteSpace(redis))
{
    builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redis);
}

// 7. Quartz.NET - one job with a cron schedule read from configuration
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("nightly-report");
    q.AddJob<NightlyReportJob>(o => o.WithIdentity(jobKey));
    q.AddTrigger(t => t
        .ForJob(jobKey)
        .WithCronSchedule(builder.Configuration["Jobs:NightlyReportCron"] ?? "0 0 2 * * ?"));
});
builder.Services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true);

// 8. Mediator - handlers are scoped because they use the DbContext
builder.Services.AddMediator(options =>
    options.ServiceLifetime = ServiceLifetime.Scoped);

builder.Services.AddScoped<IPriceService, ProductPriceService>();
builder.Services.AddScoped<OrderCalculator>();

// 9. OpenTelemetry - traces and metrics, exported over OTLP
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("Npgsql"))
    .WithMetrics(m => m.AddAspNetCoreInstrumentation())
    .UseOtlpExporter();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapProductEndpoints();
app.MapOrderEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
