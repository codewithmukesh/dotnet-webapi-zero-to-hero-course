using System.Collections.Concurrent;

// A small "external" API that the main API calls over HTTP.
// Some endpoints fail on purpose so the resilience demos have something to handle.
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<OrderStore>();
builder.Services.AddSingleton<FlakyCounter>();

var app = builder.Build();

// Used by the socket demo. No auth, returns a tiny payload.
app.MapGet("/ping", () => Results.Ok("pong"));

// Requires an API key so the DelegatingHandler demo has a reason to exist.
app.MapGet("/products", (HttpRequest request) =>
{
    if (request.Headers["X-Api-Key"] != "demo-key-123")
    {
        return Results.Unauthorized();
    }

    return Results.Ok(Product.All);
});

// Fails twice with 503, then succeeds. Reset it before each run.
app.MapGet("/flaky", (FlakyCounter counter) =>
{
    var attempt = counter.Next();
    return attempt <= 2
        ? Results.StatusCode(StatusCodes.Status503ServiceUnavailable)
        : Results.Ok(new { attempt });
});

app.MapPost("/flaky/reset", (FlakyCounter counter) =>
{
    counter.Reset();
    return Results.NoContent();
});

// Saves the order first, then returns 503 for the first two calls.
// This is what a timeout after a database commit looks like from the caller's side.
app.MapPost("/orders", (CreateOrderRequest request, OrderStore store) =>
{
    var order = store.Add(request);
    return store.Calls <= 2
        ? Results.StatusCode(StatusCodes.Status503ServiceUnavailable)
        : Results.Created($"/orders/{order.Id}", order);
});

app.MapGet("/orders", (OrderStore store) => Results.Ok(store.All));

app.MapPost("/orders/reset", (OrderStore store) =>
{
    store.Reset();
    return Results.NoContent();
});

app.Run();

public record Product(int Id, string Name, decimal Price)
{
    public static readonly Product[] All =
    [
        new(1, "Mechanical Keyboard", 129.99m),
        new(2, "4K Monitor", 449.00m),
        new(3, "USB-C Dock", 89.50m)
    ];
}

public record CreateOrderRequest(string CustomerId, int ProductId, int Quantity);

public record Order(Guid Id, string CustomerId, int ProductId, int Quantity, DateTime CreatedAtUtc);

public sealed class OrderStore
{
    private readonly ConcurrentQueue<Order> _orders = new();
    private int _calls;

    public int Calls => _calls;

    public IReadOnlyCollection<Order> All => _orders.ToArray();

    public Order Add(CreateOrderRequest request)
    {
        Interlocked.Increment(ref _calls);
        var order = new Order(Guid.NewGuid(), request.CustomerId, request.ProductId, request.Quantity, DateTime.UtcNow);
        _orders.Enqueue(order);
        return order;
    }

    public void Reset()
    {
        _orders.Clear();
        Interlocked.Exchange(ref _calls, 0);
    }
}

public sealed class FlakyCounter
{
    private int _count;

    public int Next() => Interlocked.Increment(ref _count);

    public void Reset() => Interlocked.Exchange(ref _count, 0);
}
