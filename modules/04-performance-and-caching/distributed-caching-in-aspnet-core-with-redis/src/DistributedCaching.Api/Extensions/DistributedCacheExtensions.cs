using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;

namespace DistributedCaching.Api.Extensions;

public static class DistributedCacheExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static Task SetAsync<T>(
        this IDistributedCache cache,
        string key,
        T value,
        CancellationToken cancellationToken = default)
    {
        return SetAsync(cache, key, value, new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30))
            .SetAbsoluteExpiration(TimeSpan.FromHours(1)), cancellationToken);
    }

    public static Task SetAsync<T>(
        this IDistributedCache cache,
        string key,
        T value,
        DistributedCacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, SerializerOptions));
        return cache.SetAsync(key, bytes, options, cancellationToken);
    }

    public static bool TryGetValue<T>(this IDistributedCache cache, string key, out T? value)
    {
        var val = cache.Get(key);
        value = default;
        if (val is null) return false;
        value = JsonSerializer.Deserialize<T>(val, SerializerOptions);
        return true;
    }

    public static async Task<T?> GetOrSetAsync<T>(
        this IDistributedCache cache,
        string key,
        Func<Task<T>> factory,
        DistributedCacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30))
            .SetAbsoluteExpiration(TimeSpan.FromHours(1));

        if (cache.TryGetValue(key, out T? value) && value is not null)
        {
            return value;
        }

        value = await factory();
        if (value is not null)
        {
            await cache.SetAsync(key, value, options, cancellationToken);
        }

        return value;
    }
}
