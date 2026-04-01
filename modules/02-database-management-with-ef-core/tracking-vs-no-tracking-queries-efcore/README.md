# Tracking vs. No-Tracking Queries in EF Core 10

Demonstrates the three tracking modes in EF Core 10 - tracking, no-tracking (`AsNoTracking`), and no-tracking with identity resolution (`AsNoTrackingWithIdentityResolution`) - with benchmarks and practical Web API examples.

## Resources

- **Article**: [Tracking vs. No-Tracking Queries in EF Core 10 - When to Use Each](https://codewithmukesh.com/blog/tracking-vs-no-tracking-queries-efcore/)
- **Course**: [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

## What You'll Learn

- How EF Core's change tracker works and its performance cost
- When to use `AsNoTracking()` vs `AsNoTrackingWithIdentityResolution()`
- Configuring default tracking behavior at the DbContext level
- Batch processing patterns with `ChangeTracker.Clear()`
- Real BenchmarkDotNet results comparing all three modes

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Quick Start

```bash
docker-compose up -d
dotnet run --project Tracking.Api
```

Open the Scalar API docs at the URL shown in the console output.

## License

MIT
