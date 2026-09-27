using System.Globalization;

namespace CartesianExplosion.Measure;

public sealed class Options(Dictionary<string, string> values)
{
    private const string DefaultConnection =
        "Host=localhost;Port=5432;Database=cartesian_explosion;Username=postgres;Password=postgres";

    public static Options Parse(IEnumerable<string> args)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? key = null;
        foreach (var arg in args)
        {
            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                key = arg[2..];
                values[key] = "true";
            }
            else if (key is not null)
            {
                values[key] = arg;
                key = null;
            }
        }

        return new Options(values);
    }

    public string Get(string key, string fallback) => values.GetValueOrDefault(key, fallback);

    public int GetInt(string key, int fallback) =>
        values.TryGetValue(key, out var v) ? int.Parse(v, CultureInfo.InvariantCulture) : fallback;

    public bool GetBool(string key, bool fallback) =>
        values.TryGetValue(key, out var v) ? bool.Parse(v) : fallback;

    public string ConnectionString =>
        Get("connection", Environment.GetEnvironmentVariable("ConnectionStrings__Default") ?? DefaultConnection);
}
