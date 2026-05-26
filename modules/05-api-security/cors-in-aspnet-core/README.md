# CORS in ASP.NET Core (.NET 10) - Sample

Runnable sample for the article [CORS in ASP.NET Core (.NET 10) - Handling Cross-Origin Requests Securely](https://codewithmukesh.com/blog/cors-in-aspnet-core/).

## What this sample shows

- Two named CORS policies (`AcmeFrontend`, `PublicWebhooks`) loaded from `appsettings.json`
- Correct middleware order: `UseRouting` → `UseCors` → (auth) → endpoints
- A globally-applied policy for the normal API surface
- An endpoint-specific policy via `RequireCors` for a Stripe-style webhook receiver
- `[DisableCors]` on a `/health` endpoint to opt out completely
- A custom response header (`X-Total-Count`) exposed via `WithExposedHeaders`
- Per-environment origin lists (localhost in Development, real hosts in Production)

## Run it

```bash
cd Cors.Api
dotnet run
```

Scalar opens at `https://localhost:7081/scalar/v1`.

## Test the preflight from the terminal

The Development policy whitelists `https://localhost:5173`. A correctly configured preflight returns `204 No Content` with the CORS headers:

```bash
curl -i -X OPTIONS https://localhost:7081/api/products \
  -H "Origin: https://localhost:5173" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: authorization, content-type" \
  -k
```

Expected response headers:

```
HTTP/1.1 204 No Content
Access-Control-Allow-Origin: https://localhost:5173
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, PATCH
Access-Control-Allow-Headers: Authorization, Content-Type, X-Correlation-Id
Access-Control-Allow-Credentials: true
Access-Control-Max-Age: 60
```

Send the same `OPTIONS` from an origin that is not in the policy and the CORS headers will be missing - which is what tells the browser to block the actual request.

## Test the webhook policy

The `PublicWebhooks` policy only allows `https://stripe.com`:

```bash
# Allowed
curl -i -X OPTIONS https://localhost:7081/webhooks/stripe \
  -H "Origin: https://stripe.com" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: content-type, stripe-signature" \
  -k

# Rejected (no CORS headers in the response)
curl -i -X OPTIONS https://localhost:7081/webhooks/stripe \
  -H "Origin: https://app.acme.com" \
  -H "Access-Control-Request-Method: POST" \
  -k
```

## Test the disabled-CORS endpoint

The `/health` endpoint never returns CORS headers - load balancers and monitoring agents should not be sending an `Origin` header anyway:

```bash
curl -i https://localhost:7081/health -k
```

## Endpoints

| Endpoint | Policy | Notes |
|----------|--------|-------|
| `GET /api/products` | `AcmeFrontend` (global) | Returns `X-Total-Count` (exposed header) |
| `POST /api/products` | `AcmeFrontend` (global) | Preflighted because of `Content-Type: application/json` |
| `POST /webhooks/stripe` | `PublicWebhooks` (endpoint-specific) | Locked to `https://stripe.com` |
| `GET /api/echo-origin` | `AcmeFrontend` (global) | Echoes the request `Origin` header for debugging |
| `GET /health` | `[DisableCors]` | CORS turned off entirely |

## Policy reference

`appsettings.Development.json` ships dev-friendly origins (`localhost:5173`, `localhost:4200`, `localhost:3000`).
`appsettings.json` ships the production stub (`https://app.acme.com`, `https://admin.acme.com`).
Update the lists to match your real front-end origins before deploying.
