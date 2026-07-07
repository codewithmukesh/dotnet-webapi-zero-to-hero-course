# Custom User Management in ASP.NET Core Web API (.NET 10)

Companion code for [Custom User Management in ASP.NET Core Web API](https://codewithmukesh.com/blog/custom-user-management-in-aspnet-core/).

One .NET 10 Minimal API that builds the user-management layer `MapIdentityApi` does not give you: a **custom user model**, your **own registration** (with extra fields), and a full set of **admin** and **self-service** endpoints for managing users, roles, and profiles.

Uses ASP.NET Core Identity with an EF Core **in-memory** store, so it is F5-ready with zero database setup. Scalar is at `/scalar` in development.

## Run it

```bash
dotnet run --project CustomUserManagement.Api    # http://localhost:5170
```

Then walk through `CustomUserManagement.Api/requests.http` from the top - it logs you in as the admin, registers a user, and exercises every management endpoint.

## What each piece shows

| Area | Route(s) | Point |
|------|----------|-------|
| Custom user model | `Models/ApplicationUser.cs` | Extends `IdentityUser` with `FirstName`, `LastName`, `ProfilePictureUrl`, `IsActive`, `CreatedOn`. |
| Custom registration | `POST /auth/register` | Captures the extra fields `MapIdentityApi`'s fixed `/register` cannot, assigns the default `User` role. |
| Login | `POST /auth/login` | Keeps Identity for the password check, issues your own signed JWT. Refuses disabled accounts. |
| Admin user list | `GET /api/admin/users?page=&pageSize=&search=` | Paged, searchable, each user with their roles. |
| Role management | `POST/DELETE /api/admin/users/{id}/roles` | Assign and remove roles over HTTP. |
| Lock vs disable | `POST .../lock`, `.../disable` | Two different "block this user" actions - see the article. |
| Self-service | `GET/PUT /api/me`, `POST /api/me/change-password` | A user manages their own account, id read from the JWT. |
| Email flows | `/auth/forgot-password`, `/auth/reset-password`, `/auth/confirm-email` | Token generation through an `IEmailSender` seam (logged to console here). |

## Seeded super-admin

| Email | Password | Role |
|-------|----------|------|
| `admin@codewithmukesh.com` | `Admin123!` | Admin |

## Going to a real database

Swap `UseInMemoryDatabase` in `Program.cs` for `UseSqlServer` / `UseSqlite`, then add the Identity migration:

```bash
dotnet ef migrations add InitialIdentity
dotnet ef database update
```

Because `ApplicationUser` inherits `IdentityUser`, the extra columns land in `AspNetUsers` automatically.
