# Singleton vs Scoped vs Transient in .NET 10

Source code for the lesson **[Singleton vs Scoped vs Transient in .NET 10 - Service Lifetimes Explained](https://codewithmukesh.com/blog/when-to-use-transient-scoped-singleton-dotnet/)** from the free .NET Web API Zero to Hero course.

A small .NET 10 minimal API with three services, one per lifetime:

| Service | Lifetime | Registered with |
|---|---|---|
| `ProductValidator` | Transient | `AddTransient` |
| `AppDbContext` | Scoped | `AddDbContext` (scoped by default) |
| `ProductCache` | Singleton | `AddSingleton` |

## Run it

```bash
dotnet run --project ServiceLifetimes.Api
```

The API listens on `http://localhost:5080`. Scalar is at `/scalar/v1`.

## Experiments

**1. See the lifetimes** - `GET /lifetimes`. Two classes resolve all three services; call it twice and compare the instance IDs.

```json
{"productService":{"validator":"ebbe","dbContext":"1f97","cache":"5866"},"pricingService":{"validator":"b79f","dbContext":"1f97","cache":"5866"}}
{"productService":{"validator":"246f","dbContext":"2c77","cache":"5866"},"pricingService":{"validator":"f751","dbContext":"2c77","cache":"5866"}}
```

**2. The fix** - `GET /fixed/products` (add `?refresh=true` to reload). The singleton cache creates a fresh scope, and so a fresh `DbContext`, every time it loads.

**3. The captive dependency** - set `Demo:EnableCaptiveDependency` to `true`:

```bash
dotnet run --project ServiceLifetimes.Api -- --Demo:EnableCaptiveDependency=true
```

In Development the app refuses to start with `Cannot consume scoped service 'AppDbContext' from singleton 'CaptiveProductCache'`. Run it in Production and it starts silently; `GET /captive/products` then shows the cache reusing the first `DbContext` for every request:

```bash
dotnet run --project ServiceLifetimes.Api --no-launch-profile --environment Production -- --Demo:EnableCaptiveDependency=true --urls http://localhost:5080
```

**4. Scope validation everywhere** - add `--Demo:ValidateScopesEverywhere=true` to the Production command above and the app fails at startup instead.
