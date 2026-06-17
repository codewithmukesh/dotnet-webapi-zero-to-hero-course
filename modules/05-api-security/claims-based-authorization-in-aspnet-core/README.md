# Claims-Based Authorization in ASP.NET Core (.NET 10)

Companion code for the article [Claims-Based Authorization in ASP.NET Core](https://codewithmukesh.com/blog/claims-based-authorization-in-aspnet-core/).

A minimal ASP.NET Core Web API that shows how to:

- Store custom claims against users with ASP.NET Core Identity
- Carry claims inside a JWT and read them back on each request
- Enforce claim presence (`RequireClaim("department")`) vs claim value (`RequireClaim("department", "engineering")`)
- Enrich the `ClaimsPrincipal` per request with `IClaimsTransformation` (subscription tier)
- Read claims in endpoint code with `FindFirstValue` and `HasClaim`

## Run it

```bash
dotnet run --project ClaimsBasedAuth.Api
```

The app uses the EF Core **InMemory** provider, so there is nothing to install or migrate. Open `scalar/v1` in the browser, or use `ClaimsBasedAuth.Api/requests.http`.

Three test users are seeded:

| Email | Password | Roles | Department claim | Subscription (via transformation) |
|-------|----------|-------|------------------|------------------------------------|
| `admin@codewithmukesh.com` | `Admin123!` | Admin, Manager | engineering | premium |
| `manager@codewithmukesh.com` | `Manager123!` | Manager | operations | standard |
| `user@codewithmukesh.com` | `User123!` | User | (none) | free |

## Stack

.NET 10 · Minimal APIs · ASP.NET Core Identity · EF Core 10 (InMemory) · Scalar
