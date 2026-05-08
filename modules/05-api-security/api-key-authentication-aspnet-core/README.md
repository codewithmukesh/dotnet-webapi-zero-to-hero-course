# API Key Authentication in ASP.NET Core (.NET 10)

Companion source for [API Key Authentication in ASP.NET Core (.NET 10) - Complete Guide](https://codewithmukesh.com/blog/api-key-authentication-aspnet-core/) on codewithmukesh.com.

## What's inside

A complete, production-shaped API key authentication implementation:

- Custom `AuthenticationHandler<ApiKeyAuthenticationOptions>` registered with the ASP.NET Core auth pipeline.
- EF Core 10 `ApiKey` entity with prefix, **SHA-256 key hash**, owner, scopes, expiry, revocation, and last-used timestamp.
- HybridCache validation layer (~50 ns L1 hits vs ~1-3 ms DB roundtrip).
- ProblemDetails (RFC 9457) responses for 401 / 403.
- OpenAPI 3.1 `apiKey` security scheme so Scalar's "Authorize" button works out of the box.
- Admin endpoints to issue, list, revoke, and rotate keys.
- xUnit v3 + WebApplicationFactory + FakeTimeProvider integration tests.

## Run it

```bash
dotnet run --project ApiKeyAuth.Api
```

The Api project uses EF Core's in-memory provider, so no Docker or external database is required. On first start, a bootstrap admin key is generated and **printed to the console once**:

```
warn: ApiKeyAuth.Api.Program[0]
      Bootstrap admin API key (copy now, it is only shown once): sk_live_<your-bootstrap-key>
```

Copy it. Open Scalar at `http://localhost:5000/scalar/v1`, click **Authorize**, paste the key, and try the endpoints.

## Try the endpoints

```bash
# Public endpoint - no key needed
curl http://localhost:5000/public

# Protected endpoint - returns 401 without a key
curl http://localhost:5000/secure

# Protected endpoint - works with the bootstrap key
curl -H "X-API-Key: <your-bootstrap-key>" http://localhost:5000/secure

# Issue a new key for a partner
curl -X POST http://localhost:5000/admin/keys/ \
     -H "Content-Type: application/json" \
     -d '{"name":"WeatherWidget","ownerId":"partner-weather","scopes":["keys:read"],"ttlDays":90}'
```

## Run the tests

```bash
dotnet test ApiKeyAuth.Api.Tests
```

The test project covers:

- Public endpoints don't require a key
- Missing key → 401 ProblemDetails
- Invalid key → 401
- Valid key → 200
- Revoked key → 401
- Expired key → 401
- Issue endpoint returns plaintext exactly once
- Stored value is a hash, not plaintext
- Wrong scope → 403
- Correct scope → 200

## Production checklist

When adapting this for production:

1. Swap `UseInMemoryDatabase` for `UseNpgsql` (or your DB of choice) and add a real EF Core migration.
2. Move the bootstrap admin key out of `Program.cs` and into a one-shot CLI/admin tool.
3. Protect admin endpoints behind an additional auth layer (a separate admin scheme, IP allowlist, or VPN).
4. Drop the cache TTL in `ApiKeyValidator.CacheTtl` if your revocation latency tolerance is below 2 minutes.
5. Add a key rotation reminder job (Hangfire / Quartz / hosted service) that emails owners 14 days before `ExpiresAt`.
6. Log key issuance and revocation to a structured logging sink (Serilog + Seq) with `ApiKeyPrefix` and `ClientId`.

Happy Coding :)
