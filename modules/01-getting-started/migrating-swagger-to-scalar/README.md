# Migrating from Swagger to Scalar in ASP.NET Core

The fully migrated target project - native OpenAPI + Scalar in .NET 10, with every Swashbuckle
concept ported to a transformer. Build clean and runtime-verified end to end.

## Article

**[Migrating from Swagger to Scalar in ASP.NET Core (.NET 10)](https://codewithmukesh.com/blog/migrating-from-swagger-to-scalar-aspnet-core/)**

## Course

**[.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)** - The complete FREE course for .NET Web API development.

## What This Demonstrates

- `AddOpenApi()` + `MapScalarApiReference()` replacing `AddSwaggerGen()` + `UseSwaggerUI()`
- `ApiInfoDocumentTransformer` - the native replacement for `SwaggerDoc(new OpenApiInfo {...})`
- `AuthResponsesTransformer` - an `IOpenApiOperationTransformer` replacing a Swashbuckle `IOperationFilter`
- `BearerSecuritySchemeTransformer` - the JWT security scheme so Scalar renders the authorize lock (Microsoft.OpenApi 2.0 shapes)
- XML doc comments feeding OpenAPI via `GenerateDocumentationFile` - no `IncludeXmlComments`
- Stable operation IDs with `.WithName()` so generated clients keep their method names
- Output-cached document (`CacheOutput()`) plus a YAML route at `/openapi/v1.yaml`
- Build-time generation via `Microsoft.Extensions.ApiDescription.Server` - `dotnet build` writes `MigratingSwaggerToScalar.Api.json` next to the project
- Scalar customization (theme, preferred scheme, dev-token prefill) and production hardening (no proxy, docs behind auth)

## Run It

```bash
dotnet run --project MigratingSwaggerToScalar.Api
```

Then open `/scalar` for the UI, or `/openapi/v1.json` (or `/openapi/v1.yaml`) for the raw document.

## Try the Auth Flow

1. `POST /token` with `{ "username": "mukesh" }` to get a development JWT.
2. `POST /products` without the token returns `401`.
3. Paste the token into Scalar's authorize panel (or send `Authorization: Bearer <token>`) and `POST /products` returns `201`.

## Stack

- .NET 10
- Microsoft.AspNetCore.OpenApi 10.0.12
- Microsoft.Extensions.ApiDescription.Server 10.0.12
- Scalar.AspNetCore 2.17.4
- Microsoft.AspNetCore.Authentication.JwtBearer 10.0.12
