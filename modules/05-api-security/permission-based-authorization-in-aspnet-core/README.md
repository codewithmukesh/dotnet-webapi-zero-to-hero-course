# Permission-Based Authorization in ASP.NET Core (.NET 10)

Companion code for the article [Permission-Based Authorization in ASP.NET Core - A .NET 10 Guide](https://codewithmukesh.com/blog/permission-based-authorization-in-aspnet-core/).

This is the capstone of the authorization series. It picks up where [policy-based authorization](../policy-based-authorization-in-aspnet-core) left off and replaces hard-coded policy names with permissions that an admin can change at runtime.

What it shows:

- Permissions modelled as claims on the **role**, not the user, so one grant covers everyone in that role
- A single `PermissionRequirement` + handler instead of one requirement class per rule
- `PermissionPolicyProvider` building a policy on demand for any `Permissions.*` name, so a new permission is a data change rather than a code change
- Admin endpoints that grant and revoke permissions live, with no redeploy and no restart
- Two permission delivery strategies behind one config switch, so you can watch the difference
- A diagnostics probe that measures what putting permissions in the token actually costs

## Run it

```
dotnet run --project PermissionBasedAuth.Api
```

Scalar opens at `/scalar/v1`. There is no database to set up - Identity runs on the EF Core in-memory provider and reseeds on every start.

## Seeded users

| Email | Password | Role | Permissions |
|-------|----------|------|-------------|
| `admin@codewithmukesh.com` | `Admin123!` | Admin | View, Create, Edit, Delete |
| `manager@codewithmukesh.com` | `Manager123!` | Manager | View, Create |
| `user@codewithmukesh.com` | `User123!` | User | View |

Each user passes some endpoints and fails others, so you can see 200, 401, and 403 without editing anything.

## The config switch that matters

`appsettings.json`:

```json
"Permissions": { "Source": "Lookup" }
```

- **`Lookup`** (default) - only roles travel in the token. Permissions are resolved per request from the store and cached. Small tokens, and a revoke takes effect on the next request.
- **`Token`** - permissions are stamped into the JWT at login. No lookup, but the token grows with every permission and a revoked permission keeps working until the token expires.

Walk `requests.http` top to bottom under each setting. The revoke step behaves differently, and that difference is the point.

## Stack

.NET 10 · Minimal APIs · ASP.NET Core Identity · EF Core 10 (InMemory) · Scalar
