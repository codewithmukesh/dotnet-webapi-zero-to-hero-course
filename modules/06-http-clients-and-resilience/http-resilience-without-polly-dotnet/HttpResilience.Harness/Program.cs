using System.Diagnostics;
using System.Net.Http.Json;
using HttpResilience.FlakyApi;
using HttpResilience.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// 1. Start the flaky downstream API in-process.
const string baseUrl = "http://localhost:5199";
var flakyBuilder = WebApplication.CreateBuilder();
flakyBuilder.Logging.ClearProviders();
flakyBuilder.WebHost.UseUrls(baseUrl);
var flaky = flakyBuilder.Build();
flaky.MapFlakyEndpoints();
await flaky.StartAsync();

// 2. Register three clients with the same settings:
//    10s total timeout, 1s attempt timeout, 3 retries from a 200ms base delay, 5s break.
var services = new ServiceCollection();
services.AddLogging();

void AddHandRolled(string name) => services.AddHttpClient(name, c => c.BaseAddress = new Uri(baseUrl))
    .AddHandRolledResilience(o =>
    {
        o.TotalTimeout = TimeSpan.FromSeconds(10);
        o.AttemptTimeout = TimeSpan.FromSeconds(1);
        o.MaxRetries = 3;
        o.BaseDelay = TimeSpan.FromMilliseconds(200);
        o.FailureThreshold = 5;
        o.BreakDuration = TimeSpan.FromSeconds(5);
    });

void AddMicrosoft(string name) => services.AddHttpClient(name, c => c.BaseAddress = new Uri(baseUrl))
    .AddStandardResilienceHandler(o =>
    {
        o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
        o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(1);
        o.Retry.MaxRetryAttempts = 3;
        o.Retry.Delay = TimeSpan.FromMilliseconds(200);
        o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
        o.CircuitBreaker.MinimumThroughput = 5;
        o.CircuitBreaker.FailureRatio = 0.5;
        o.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(5);
    });

void AddKevlar(string name) => services.AddHttpClient(name, c => c.BaseAddress = new Uri(baseUrl))
    .AddStandardShield(o =>
    {
        o.TotalTimeout!.Timeout = TimeSpan.FromSeconds(10);
        o.AttemptTimeout!.Timeout = TimeSpan.FromSeconds(1);
        o.Retry!.MaxRetries = 3;
        o.Retry.Backoff = Kevlar.Backoff.Exponential(TimeSpan.FromMilliseconds(200));
        o.CircuitBreaker!.SamplingWindow = TimeSpan.FromSeconds(10);
        o.CircuitBreaker.MinimumThroughput = 5;
        o.CircuitBreaker.FailureRatio = 0.5;
        o.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(5);
    });

string[] scenarios = ["flaky", "throttled", "slow", "post", "down"];
foreach (var scenario in scenarios)
{
    // A separate named client per scenario, so one scenario's failures
    // cannot leave another scenario's circuit breaker open.
    AddHandRolled($"hand-rolled:{scenario}");
    AddMicrosoft($"microsoft:{scenario}");
    AddKevlar($"kevlar:{scenario}");
}

using var provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IHttpClientFactory>();
var probe = new HttpClient { BaseAddress = new Uri(baseUrl) };
string[] clients = ["hand-rolled", "microsoft", "kevlar"];

Console.WriteLine($"Runtime: {Environment.Version}, OS: {Environment.OSVersion}");
Console.WriteLine();
Console.WriteLine("| Scenario | Client | Result | Server hits | Time (ms) |");
Console.WriteLine("|---|---|---|---|---|");

// 3. Single-request scenarios.
foreach (var client in clients)
{
    HttpClient Http(string scenario) => factory.CreateClient($"{client}:{scenario}");
    await Single("503 twice, then 200", client, id => Http("flaky").GetAsync($"/flaky/{id}?failures=2"));
    await Single("429 + Retry-After: 1", client, id => Http("throttled").GetAsync($"/throttled/{id}"));
    await Single("first call hangs 5s", client, id => Http("slow").GetAsync($"/slow/{id}?hangMs=5000"));
    await Single("POST, 503 once", client, id => Http("post").PostAsJsonAsync($"/orders/{id}", new { sku = "abc" }));
}

// 4. A dead service: 20 calls in a row. How many reach the server, and how long does it take?
foreach (var client in clients)
{
    var http = factory.CreateClient($"{client}:down");
    var id = $"{client}-down-{Guid.NewGuid():N}";
    var outcomes = new Dictionary<string, int>();
    var sw = Stopwatch.StartNew();
    for (var i = 0; i < 20; i++)
    {
        var outcome = await Outcome(() => http.GetAsync($"/down/{id}"));
        outcomes[outcome] = outcomes.GetValueOrDefault(outcome) + 1;
    }
    sw.Stop();
    var hits = await probe.GetFromJsonAsync<int>($"/hits/{id}");
    var summary = string.Join(", ", outcomes.Select(o => $"{o.Key} x{o.Value}"));
    Console.WriteLine($"| service down, 20 calls | {client} | {summary} | {hits} | {sw.ElapsedMilliseconds} |");
}

await flaky.StopAsync();
return;

async Task Single(string scenario, string client, Func<string, Task<HttpResponseMessage>> call)
{
    var id = $"{client}-{Guid.NewGuid():N}";
    var sw = Stopwatch.StartNew();
    var outcome = await Outcome(() => call(id));
    sw.Stop();
    var hits = await probe.GetFromJsonAsync<int>($"/hits/{id}");
    Console.WriteLine($"| {scenario} | {client} | {outcome} | {hits} | {sw.ElapsedMilliseconds} |");
}

static async Task<string> Outcome(Func<Task<HttpResponseMessage>> call)
{
    try
    {
        using var response = await call();
        return ((int)response.StatusCode).ToString();
    }
    catch (Exception ex)
    {
        return ex.GetType().Name;
    }
}
