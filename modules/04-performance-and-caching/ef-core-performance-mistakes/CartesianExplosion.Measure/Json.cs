using System.Text.Json;

namespace CartesianExplosion.Measure;

public static class Json
{
    private static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

    public static async Task WriteAsync(string path, object value, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(value, Indented), ct);
    }

    public static string Line(object value) => JsonSerializer.Serialize(value);
}
