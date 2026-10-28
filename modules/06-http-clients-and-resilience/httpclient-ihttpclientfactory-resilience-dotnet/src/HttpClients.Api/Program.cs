using HttpClients.Api.Clients;
using HttpClients.Api.Handlers;
using HttpClients.Api.Options;
using HttpClients.Api.Services;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddOptions<CatalogApiOptions>()
    .BindConfiguration(CatalogApiOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// DelegatingHandlers are resolved from DI, so register them as transient.
builder.Services.AddTransient<ApiKeyHandler>();
builder.Services.AddTransient<OuterTraceHandler>();
builder.Services.AddTransient<InnerTraceHandler>();
builder.Services.AddSingleton<HandlerTrace>();

// 1. Named client "catalog", registered as a keyed service (.NET 9+).
//    CatalogClient receives it through [FromKeyedServices("catalog")], and
//    IHttpClientFactory.CreateClient("catalog") returns a client with exactly the same configuration.
builder.Services.AddHttpClient("catalog", (sp, client) =>
    {
        var options = sp.GetRequiredService<IOptions<CatalogApiOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseAddress);
    })
    .AddAsKeyed()
    .AddHttpMessageHandler<ApiKeyHandler>()
    .AddStandardResilienceHandler();

builder.Services.AddTransient<CatalogClient>();

// 2. Named client with the standard resilience handler and its default options.
//    The defaults retry every HTTP method, including POST.
builder.Services.AddHttpClient("orders-default", client =>
    {
        client.BaseAddress = new Uri("http://localhost:5100/");
    })
    .AddStandardResilienceHandler();

// 3. Same client, but retries are switched off for POST, PUT, PATCH, DELETE and CONNECT.
builder.Services.AddHttpClient("orders-safe", client =>
    {
        client.BaseAddress = new Uri("http://localhost:5100/");
    })
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.DisableForUnsafeHttpMethods();
    });

// 4. Keyed client (.NET 9+) used for the handler order demo.
//    OuterTraceHandler is added before the resilience handler, InnerTraceHandler after it.
var flakyClient = builder.Services.AddHttpClient("flaky", client =>
    {
        client.BaseAddress = new Uri("http://localhost:5100/");
    })
    .AddAsKeyed();

flakyClient.AddHttpMessageHandler<OuterTraceHandler>();
flakyClient.AddStandardResilienceHandler();
flakyClient.AddHttpMessageHandler<InnerTraceHandler>();

builder.Services.AddHostedService<ProductSyncService>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// The keyed-client wrapper class injected straight into the endpoint.
app.MapGet("/products", async (CatalogClient catalog, CancellationToken cancellationToken) =>
    Results.Ok(await catalog.GetProductsAsync(cancellationToken)));

// Places one order through the chosen client, then counts how many orders the downstream API saved.
app.MapPost("/demo/orders/{mode}", async (string mode, IHttpClientFactory factory, CancellationToken cancellationToken) =>
{
    var clientName = mode == "safe" ? "orders-safe" : "orders-default";
    var client = factory.CreateClient(clientName);

    await client.PostAsync("orders/reset", null, cancellationToken);

    var order = new { CustomerId = "customer-42", ProductId = 1, Quantity = 1 };
    using var response = await client.PostAsJsonAsync("orders", order, cancellationToken);

    var saved = await client.GetFromJsonAsync<List<object>>("orders", cancellationToken) ?? [];

    return Results.Ok(new
    {
        client = clientName,
        finalStatusCode = (int)response.StatusCode,
        ordersPlacedByUser = 1,
        ordersSavedDownstream = saved.Count
    });
});

// Calls an endpoint that fails twice with 503 and then succeeds, and returns what each handler saw.
app.MapGet("/demo/handler-order", async (
    [FromKeyedServices("flaky")] HttpClient client,
    HandlerTrace trace,
    CancellationToken cancellationToken) =>
{
    await client.PostAsync("flaky/reset", null, cancellationToken);
    trace.Drain();

    using var response = await client.GetAsync("flaky", cancellationToken);

    return Results.Ok(new
    {
        finalStatusCode = (int)response.StatusCode,
        trace = trace.Drain()
    });
});

app.Run();
