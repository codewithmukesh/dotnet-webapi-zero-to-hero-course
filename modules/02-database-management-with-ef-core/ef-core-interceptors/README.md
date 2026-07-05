# EF Core Interceptors (.NET 10)

Companion code for the article [EF Core Interceptors: The Complete Guide (.NET 10)](https://codewithmukesh.com/blog/ef-core-interceptors/).

A minimal .NET 10 Web API that shows the two interceptors you will actually use:

- **`AuditableInterceptor`** (`ISaveChangesInterceptor`) - stamps `CreatedAtUtc` / `CreatedBy` / `UpdatedAtUtc` / `UpdatedBy` on every save and converts deletes into soft deletes. It depends on the request-scoped `ICurrentUser`, so it is registered as a **scoped** service and resolved through the `AddDbContext((sp, options) => ...)` overload.
- **`SlowQueryInterceptor`** (`IDbCommandInterceptor`) - logs any SQL command slower than 500ms. Stateless, so it is registered as a **singleton**.

The soft-delete write (the interceptor) is paired with a **global query filter** (`AppDbContext.OnModelCreating`) that hides deleted rows on read.

## Stack

- .NET 10 / EF Core 10
- PostgreSQL 17 (via Docker Compose)
- Scalar for API docs

## Run it

```bash
docker compose up -d        # start PostgreSQL
dotnet run --project EfCoreInterceptors.Api
```

Open `http://localhost:5050/scalar/v1`.

## Try the interceptors

This sample has no authentication, so `CurrentUser` falls back to an `X-User-Id`
request header. Send that header to watch the audit fields fill in.

```bash
# Create - CreatedAtUtc + CreatedBy get stamped by the interceptor
curl -X POST http://localhost:5050/products \
  -H "Content-Type: application/json" -H "X-User-Id: mukesh" \
  -d '{ "name": "Keyboard", "price": 49.99 }'

# Update - UpdatedAtUtc + UpdatedBy get stamped (DetectChanges makes this work)
curl -X PUT http://localhost:5050/products/1 \
  -H "Content-Type: application/json" -H "X-User-Id: alice" \
  -d '{ "name": "Mechanical Keyboard", "price": 89.99 }'

# Delete - becomes a soft delete (IsDeleted = true, row stays in the table)
curl -X DELETE http://localhost:5050/products/1

# List - soft-deleted rows are hidden by the global query filter
curl http://localhost:5050/products

# Deleted - bypass the filter with IgnoreQueryFilters to see them
curl http://localhost:5050/products/deleted
```

## Key files

- `Data/AuditableInterceptor.cs` - audit fields + soft delete on `SaveChanges`
- `Data/SlowQueryInterceptor.cs` - slow SQL logging on command execution
- `Data/AppDbContext.cs` - the global query filter for soft deletes
- `Services/CurrentUser.cs` - the scoped current-user service
- `Program.cs` - the scoped-vs-singleton interceptor registration
