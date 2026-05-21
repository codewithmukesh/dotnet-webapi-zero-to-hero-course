# Rate Limiting in ASP.NET Core (.NET 10) - Sample

Runnable sample for the article [Rate Limiting in ASP.NET Core (.NET 10) - Complete Guide](https://codewithmukesh.com/blog/rate-limiting-aspnet-core/).

## What this sample shows

- All four built-in algorithms: Fixed Window, Sliding Window, Token Bucket, Concurrency Limiter
- Production-grade `OnRejected` returning RFC 9457 ProblemDetails + `Retry-After` header
- Multi-tenant tier resolution (Free / Pro / Enterprise) partitioned by `X-API-Key`
- Optional Redis backplane for multi-instance enforcement (off by default)
- `[DisableRateLimiting]` for health endpoints

## Run it

```bash
cd RateLimiting.Api
dotnet run
```

Open Scalar at `https://localhost:5xxx/scalar/v1` to explore the endpoints.

## Try the rate limiter

The Free-tier API key is in `appsettings.json` as `free-key-001` (60 tokens/min):

```bash
# This will be rate-limited around the 60th call inside a minute
for i in {1..70}; do
  curl -i -H "X-API-Key: free-key-001" https://localhost:5xxx/api/pricing
done
```

The 61st response is:

```
HTTP/2 429
content-type: application/problem+json
retry-after: 58

{
  "type": "https://datatracker.ietf.org/doc/html/rfc6585#section-4",
  "title": "Too many requests",
  "status": 429,
  "detail": "...",
  "instance": "/api/pricing",
  "traceId": "..."
}
```

## Enable the Redis backplane

In `appsettings.json`:

```json
"RateLimiting": {
  "EnableRedisBackplane": true,
  "Redis": "localhost:6379"
}
```

Then start a local Redis: `docker run -p 6379:6379 redis:7-alpine`.

The Redis-backed policy is exposed as `public-api-redis`. Multiple instances of the app sharing the same Redis backplane will enforce the limit collectively.

## Endpoints

| Endpoint | Policy | Algorithm | Use case |
|----------|--------|-----------|----------|
| `GET /api/pricing` | `public-api` | Token Bucket | Public API default |
| `GET /api/internal/health` | `internal-rpc` | Fixed Window | Trusted internal RPC |
| `POST /api/login` | `auth-endpoints` | Sliding Window | Security-sensitive auth |
| `POST /api/upload` | `file-upload` | Concurrency | In-flight cap on expensive work |
| `GET /health` | (disabled) | - | Always allowed |

## Multi-tenant tiers

The global limiter resolves the API key on every request and partitions accordingly:

| Tier | Token Limit | Refill | API key (demo) |
|------|------------|--------|----------------|
| Anonymous (no key) | 20 / min | Fixed Window | (none) |
| Invalid key | 5 / min | Fixed Window | any unknown value |
| Free | 60 / min | Token Bucket | `free-key-001` |
| Pro | 600 / min | Token Bucket | `pro-key-002` |
| Enterprise | 6000 / min | Token Bucket | `enterprise-key-003` |
