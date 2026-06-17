# Identity API Endpoints in ASP.NET Core (.NET 10)

Companion code for [Identity API Endpoints in ASP.NET Core: When to Use Them](https://codewithmukesh.com/blog/identity-endpoints-aspnet-core/).

Two small APIs that show the two paths from the article side by side:

| Project | What it shows |
|---------|---------------|
| `IdentityEndpoints.Api` | The `MapIdentityApi()` happy path - ten endpoints in one line, issuing an **opaque** bearer token. |
| `CustomJwt.Api` | The escape hatch - keep ASP.NET Core Identity for the user store, but issue your **own signed JWT** with `SignInManager` + a `TokenService`. |

Both use ASP.NET Core Identity with an EF Core **in-memory** store, so they are F5-ready with zero database setup. Both expose Scalar at `/scalar` in development.

## Run it

```bash
# Path 1: MapIdentityApi (opaque token)
dotnet run --project IdentityEndpoints.Api        # http://localhost:5150

# Path 2: custom JWT (escape hatch)
dotnet run --project CustomJwt.Api                # http://localhost:5160
```

Walk through each project's `requests.http` to exercise every endpoint.

## The one thing to notice

Log in on each project and compare the `accessToken`:

- **`IdentityEndpoints.Api`** -> paste the token into [jwt.io](https://jwt.io/). It does **not** decode. It is an encrypted, app-only blob. No readable claims, nothing another service can validate.
- **`CustomJwt.Api`** -> paste the token into jwt.io. It decodes, and the payload contains `"role": "Admin"`. Any service that trusts the signing key can read it.

That single difference is the whole decision in the article: the free endpoints are perfect for a first-party app, and a wall the moment a second service needs to trust the token.

## Seeded users (CustomJwt.Api)

| Email | Password | Role |
|-------|----------|------|
| `admin@codewithmukesh.com` | `Admin123!` | Admin |
| `user@codewithmukesh.com` | `User123!` | (none) |

`IdentityEndpoints.Api` has no seeded users - register one via `POST /register`.

Tested on .NET 10 with `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.0, `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.0, and `Scalar.AspNetCore` 2.13.18.
