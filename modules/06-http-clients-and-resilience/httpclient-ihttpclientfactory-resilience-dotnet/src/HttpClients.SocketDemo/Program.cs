using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.Extensions.DependencyInjection;

// Usage: dotnet run -- new      (a new HttpClient for every request)
//        dotnet run -- factory  (IHttpClientFactory)
// Start HttpClients.Downstream first. Wait a few minutes between runs so old TIME_WAIT sockets expire.
var mode = args.FirstOrDefault() ?? "new";
const int requestCount = 500;
const int targetPort = 5100;
var url = new Uri($"http://localhost:{targetPort}/ping");

var services = new ServiceCollection();
services.AddHttpClient();
using var provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IHttpClientFactory>();

var before = CountSockets(targetPort);
var stopwatch = Stopwatch.StartNew();

for (var i = 0; i < requestCount; i++)
{
    if (mode == "new")
    {
        // The pattern to avoid: a new client, and a new connection pool, for every request.
        using var client = new HttpClient();
        using var response = await client.GetAsync(url);
    }
    else
    {
        // The factory hands out a new HttpClient, but reuses the pooled handler underneath it.
        var client = factory.CreateClient();
        using var response = await client.GetAsync(url);
    }
}

stopwatch.Stop();
var after = CountSockets(targetPort);

Console.WriteLine($"Mode: {mode}");
Console.WriteLine($"Requests sent: {requestCount}");
Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds} ms");
Console.WriteLine($"New TIME_WAIT sockets to port {targetPort}: {after.TimeWait - before.TimeWait}");
Console.WriteLine($"Established connections to port {targetPort}: {after.Established}");

static (int TimeWait, int Established) CountSockets(int port)
{
    var connections = IPGlobalProperties.GetIPGlobalProperties()
        .GetActiveTcpConnections()
        .Where(c => c.RemoteEndPoint.Port == port)
        .ToList();

    return (
        connections.Count(c => c.State == TcpState.TimeWait),
        connections.Count(c => c.State == TcpState.Established));
}
