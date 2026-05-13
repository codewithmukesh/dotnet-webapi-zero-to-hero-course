# AutoMapper vs Mapster vs Mapperly vs Manual Mapping

Reference implementation for the article [AutoMapper vs Mapster vs Manual Mapping in .NET 10](https://codewithmukesh.com/blog/automapper-vs-mapster-vs-manual-mapping-dotnet/).

## Layout

- `Mapping.Api/` - ASP.NET Core Minimal API exposing one endpoint per mapper:
  - `GET /products/automapper`
  - `GET /products/mapster`
  - `GET /products/mapperly`
  - `GET /products/manual`
- `Mapping.Benchmarks/` - BenchmarkDotNet project measuring all four mappers on the same `Product -> ProductResponse` projection (nested `Category` + 3 `Tag` items).

## Run the API

```bash
cd Mapping.Api
dotnet run -c Release
```

Open the Scalar UI at `http://localhost:5099/scalar/v1` and hit each endpoint to confirm they return identical JSON.

## Run the benchmarks

```bash
cd Mapping.Benchmarks
dotnet run -c Release
```

Mean time, ratio against the manual baseline, and allocated bytes are reported per mapper. The full output also lands in `BENCHMARKS.md`.

## Packages

- AutoMapper 14.0.0 (last MIT version, has known security advisory GHSA-rvv3-g6hj-g44x; fix is in commercial v15+)
- Mapster 7.4.0 (MIT)
- Riok.Mapperly 4.2.1 (Apache-2.0, source generator)
- BenchmarkDotNet 0.15.4
- Scalar.AspNetCore 2.11.9

## License

MIT - same as the rest of the course.
