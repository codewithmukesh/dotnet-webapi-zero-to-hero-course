# JWT Authentication in ASP.NET Core (.NET 10)

Companion code for the article [JWT Authentication in ASP.NET Core](https://codewithmukesh.com/blog/jwt-authentication-in-aspnet-core/).

A minimal ASP.NET Core Web API that shows how to:

- Register and log in users with ASP.NET Core Identity
- Generate signed JWTs with `JsonWebTokenHandler`
- Protect endpoints with `[Authorize]` and role checks
- Add roles to users (admin-only)

## Run it

```bash
dotnet run --project JwtAuthentication.Api
```

The app uses the EF Core **InMemory** provider, so there is nothing to install or migrate. Open `scalar/v1` in the browser, or use `JwtAuthentication.Api/requests.http`.

A default admin is seeded for you:

- Email: `admin@codewithmukesh.com`
- Password: `Admin123!`

## Stack

.NET 10 · Minimal APIs · ASP.NET Core Identity · EF Core 10 (InMemory) · Scalar
