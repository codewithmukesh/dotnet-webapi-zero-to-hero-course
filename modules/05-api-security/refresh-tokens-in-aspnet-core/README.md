# Refresh Tokens in ASP.NET Core (.NET 10)

Companion code for the article [Refresh Tokens in ASP.NET Core](https://codewithmukesh.com/blog/refresh-tokens-in-aspnet-core/). This is Part 2 of [JWT Authentication in ASP.NET Core](https://codewithmukesh.com/blog/jwt-authentication-in-aspnet-core/).

A minimal ASP.NET Core Web API that shows how to:

- Issue a short-lived JWT access token plus a long-lived refresh token on login
- Exchange a refresh token for a new access token with **token rotation**
- Detect refresh token **reuse** (theft) and revoke the whole token family
- **Revoke** a refresh token (logout)

## Run it

```bash
dotnet run --project RefreshTokens.Api
```

The app uses the EF Core **InMemory** provider, so there is nothing to install or migrate. Open `scalar/v1` in the browser, or use `RefreshTokens.Api/requests.http`.

A default admin is seeded for you:

- Email: `admin@codewithmukesh.com`
- Password: `Admin123!`

## Stack

.NET 10 · Minimal APIs · ASP.NET Core Identity · EF Core 10 (InMemory) · Scalar
