using System.Collections.Concurrent;
using System.Diagnostics;

namespace CartesianExplosion.Measure;

public static class Load
{
    public sealed record Sample(double Ms, int Status, string? Error);

    public static string Classify(Sample s) => s.Error switch
    {
        "TaskCanceledException" => "client-timeout",
        not null => "failed",
        _ when s.Status is >= 200 and < 300 => "ok",
        _ => $"http-{s.Status}",
    };

    /// <summary>
    /// `load --url http://localhost:5080 --mode single --documents true --concurrency 200 --seconds 30`
    /// Hammers one endpoint with N workers while a probe hits /health every 250 ms.
    /// </summary>
    public static async Task<int> RunAsync(Options options, CancellationToken ct)
    {
        var url = options.Get("url", "http://localhost:5080");
        var mode = options.Get("mode", "single");
        var documents = options.GetBool("documents", true);
        var concurrency = options.GetInt("concurrency", 200);
        var seconds = options.GetInt("seconds", 30);

        using var http = new HttpClient { BaseAddress = new Uri(url), Timeout = TimeSpan.FromSeconds(60) };
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(ct);
        stop.CancelAfter(TimeSpan.FromSeconds(seconds));

        var main = new ConcurrentBag<Sample>();
        var health = new ConcurrentBag<Sample>();
        string? firstError = null;
        var path = $"/departments/{mode}?documents={documents.ToString().ToLowerInvariant()}";

        var workers = Enumerable.Range(0, concurrency).Select(_ => Task.Run(async () =>
        {
            while (!stop.IsCancellationRequested)
            {
                var (sample, body) = await SendAsync(http, path, stop.Token);
                if (sample is null)
                {
                    continue;
                }

                main.Add(sample);
                if (sample.Status >= 500 && body is not null)
                {
                    Interlocked.CompareExchange(ref firstError, body, null);
                }
            }
        })).ToArray();

        var probe = Task.Run(async () =>
        {
            while (!stop.IsCancellationRequested)
            {
                var (sample, _) = await SendAsync(http, "/health", stop.Token);
                if (sample is not null)
                {
                    health.Add(sample);
                }

                try
                {
                    await Task.Delay(250, stop.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        });

        await Task.WhenAll(workers.Append(probe));

        var report = new
        {
            mode,
            documents,
            concurrency,
            seconds,
            main = Summarize(main),
            health = Summarize(health),
        };
        await Json.WriteAsync($"results/load-{mode}{(documents ? "-documents" : "")}-c{concurrency}.json", report, ct);
        if (firstError is not null)
        {
            await File.WriteAllTextAsync("results/load-first-error.txt", firstError, ct);
        }

        Console.WriteLine(Json.Line(report));
        return 0;
    }

    private static async Task<(Sample? Sample, string? Body)> SendAsync(HttpClient http, string path, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var response = await http.GetAsync(path, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            var status = (int)response.StatusCode;
            return (new Sample(stopwatch.Elapsed.TotalMilliseconds, status, null), status >= 500 ? body : null);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return (null, null); // the run ended mid-request: not a sample
        }
        catch (Exception ex)
        {
            return (new Sample(stopwatch.Elapsed.TotalMilliseconds, 0, ex.GetType().Name), null);
        }
    }

    private static object Summarize(IReadOnlyCollection<Sample> samples)
    {
        var ms = samples.Select(s => s.Ms).ToArray();
        return new
        {
            count = samples.Count,
            p50Ms = Math.Round(Stats.Percentile(ms, 50)),
            p95Ms = Math.Round(Stats.Percentile(ms, 95)),
            maxMs = ms.Length == 0 ? 0 : Math.Round(ms.Max()),
            outcomes = samples.GroupBy(Classify).ToDictionary(g => g.Key, g => g.Count()),
        };
    }
}
