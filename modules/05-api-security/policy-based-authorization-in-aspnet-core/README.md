# Policy-Based Authorization in ASP.NET Core (.NET 10)

Companion code for the article [Policy-Based Authorization in ASP.NET Core](https://codewithmukesh.com/blog/policy-based-authorization-in-aspnet-core/).

A minimal ASP.NET Core Web API that shows how to:

- Define named policies with `AddAuthorizationBuilder` (requirements + handlers)
- Run TWO handlers for one requirement (OR semantics: premium subscription OR Admin role)
- Inject dependencies into handlers (`TimeProvider` for a working-hours rule)
- Carry requirement data on an attribute with `IAuthorizationRequirementData` (`[MinimumTenure(6)]`)
- Secure the whole API by default with a fallback policy
- Evaluate a policy imperatively with `IAuthorizationService` for custom failure responses

## Run it

```bash
dotnet run --project PolicyBasedAuth.Api
```

The app uses the EF Core **InMemory** provider, so there is nothing to install or migrate. Open `scalar/v1` in the browser, or use `PolicyBasedAuth.Api/requests.http`.

Three test users are seeded - each one passes some policies and fails others:

| Email | Password | Roles | Tier | Joined | ProAccess | Veterans (6 mo) |
|-------|----------|-------|------|--------|-----------|------------------|
| `admin@codewithmukesh.com` | `Admin123!` | Admin, Manager | standard | 2024-01-15 | ✅ via role | ✅ |
| `manager@codewithmukesh.com` | `Manager123!` | Manager | premium | 2026-04-20 | ✅ via subscription | ❌ |
| `user@codewithmukesh.com` | `User123!` | User | free | 2024-08-01 | ❌ | ✅ |

## Stack

.NET 10 · Minimal APIs · ASP.NET Core Identity · EF Core 10 (InMemory) · Scalar
