# Role-Based Authorization in ASP.NET Core (.NET 10)

Companion code for the article [Role-Based Authorization in ASP.NET Core](https://codewithmukesh.com/blog/role-based-authorization-in-aspnet-core/).

A minimal ASP.NET Core Web API that shows how to:

- Put role claims inside a JWT and have ASP.NET Core read them back
- Protect Minimal API endpoints with `RequireRole`
- Require ANY of several roles (OR) vs ALL of them (AND)
- Reuse a named role policy across endpoints
- Branch on roles in code with `User.IsInRole()`

## Run it

```bash
dotnet run --project RoleBasedAuth.Api
```

The app uses the EF Core **InMemory** provider, so there is nothing to install or migrate. Open `scalar/v1` in the browser, or use `RoleBasedAuth.Api/requests.http`.

Three test users are seeded:

| Email | Password | Roles |
|-------|----------|-------|
| `admin@codewithmukesh.com` | `Admin123!` | Admin, Manager |
| `manager@codewithmukesh.com` | `Manager123!` | Manager |
| `user@codewithmukesh.com` | `User123!` | User |

## Stack

.NET 10 · Minimal APIs · ASP.NET Core Identity · EF Core 10 (InMemory) · Scalar
