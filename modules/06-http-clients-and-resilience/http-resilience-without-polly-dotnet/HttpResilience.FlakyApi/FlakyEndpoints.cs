using System.Collections.Concurrent;

namespace HttpResilience.FlakyApi;

// A fake downstream service that fails on purpose. Each scenario uses its own id,
// so the hit counter tells you exactly how many requests reached the server.
public static class FlakyEndpoints
{
    private static readonly ConcurrentDictionary<string, int> Hits = new();

    private static int Hit(string id) => Hits.AddOrUpdate(id, 1, (_, count) => count + 1);

    public static WebApplication MapFlakyEndpoints(this WebApplication app)
    {
        // Fails with 503 for the first N calls, then succeeds.
        app.MapGet("/flaky/{id}", (string id, int failures) =>
            Hit(id) <= failures ? Results.StatusCode(503) : Results.Ok(new { id, message = "ok" }));

        // Returns 429 with Retry-After: 1 on the first call, then succeeds.
        app.MapGet("/throttled/{id}", (string id, HttpResponse response) =>
        {
            if (Hit(id) == 1)
            {
                response.Headers.RetryAfter = "1";
                return Results.StatusCode(429);
            }
            return Results.Ok(new { id, message = "ok" });
        });

        // Hangs for hangMs on the first call, then answers fast.
        app.MapGet("/slow/{id}", async (string id, int hangMs, CancellationToken ct) =>
        {
            if (Hit(id) == 1) await Task.Delay(hangMs, ct);
            return Results.Ok(new { id, message = "ok" });
        });

        // A POST that fails once with 503, then creates the order.
        app.MapPost("/orders/{id}", (string id) =>
            Hit(id) == 1 ? Results.StatusCode(503) : Results.Created($"/orders/{id}", new { id }));

        // A service that is completely down.
        app.MapGet("/down/{id}", (string id) =>
        {
            Hit(id);
            return Results.StatusCode(500);
        });

        app.MapGet("/hits/{id}", (string id) => Results.Ok(Hits.GetValueOrDefault(id)));

        return app;
    }
}
