using ApiKeyAuth.Api.Authentication;
using ApiKeyAuth.Api.Data;
using ApiKeyAuth.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace ApiKeyAuth.Api.Validation;

public sealed class ApiKeyValidator(
    AppDbContext db,
    HybridCache cache,
    TimeProvider time,
    ILogger<ApiKeyValidator> logger) : IApiKeyValidator
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(2);

    public async Task<ApiKeyValidationResult> ValidateAsync(
        string plaintextKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(plaintextKey))
            return ApiKeyValidationResult.Invalid("API key is empty.");

        var prefix = plaintextKey.Length >= 12 ? plaintextKey[..12] : plaintextKey;
        var hash = ApiKeyHasher.Hash(plaintextKey);

        var cached = await cache.GetOrCreateAsync(
            $"apikey:{hash}",
            async cancel => await LookupAsync(hash, cancel),
            new HybridCacheEntryOptions { Expiration = CacheTtl },
            cancellationToken: ct);

        if (cached is null)
            return ApiKeyValidationResult.Invalid("API key not found.", prefix);

        if (cached.RevokedAt is not null)
            return ApiKeyValidationResult.Invalid("API key has been revoked.", prefix);

        if (cached.ExpiresAt is { } exp && exp <= time.GetUtcNow())
            return ApiKeyValidationResult.Invalid("API key has expired.", prefix);

        _ = TouchLastUsedAsync(cached.Id);

        var scopes = string.IsNullOrEmpty(cached.Scopes)
            ? Array.Empty<string>()
            : cached.Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new ApiKeyValidationResult(
            true, null, cached.Id, cached.Name, cached.OwnerId, cached.Prefix, scopes);
    }

    private async Task<ApiKey?> LookupAsync(string hash, CancellationToken ct)
        => await db.ApiKeys.AsNoTracking()
            .FirstOrDefaultAsync(k => k.KeyHash == hash, ct);

    private async Task TouchLastUsedAsync(Guid keyId)
    {
        try
        {
            await db.ApiKeys
                .Where(k => k.Id == keyId)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(k => k.LastUsedAt, time.GetUtcNow().UtcDateTime));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to update LastUsedAt for key {KeyId}", keyId);
        }
    }
}
