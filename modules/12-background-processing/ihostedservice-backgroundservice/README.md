# IHostedService & BackgroundService — Module 12 Lesson 1

Runnable .NET 10 sample for the article [Understanding IHostedService & BackgroundService in .NET 10](https://codewithmukesh.com/blog/ihostedservice-vs-backgroundservice-dotnet/).

## What this demonstrates

Five hosted services registered in one ASP.NET Core 10 host, each illustrating a different pattern from the article:

| Service | Type | Demonstrates |
|---|---|---|
| `StartupTask` | `IHostedService` | One-shot startup work (raw interface, no loop) |
| `HeartbeatHostedService` | `IHostedService` | Same heartbeat loop, manually plumbed (~30 lines) |
| `HeartbeatBackgroundService` | `BackgroundService` | Same heartbeat loop, framework-plumbed (~6 lines) |
| `OrderProcessor` | `BackgroundService` | Gotcha 2 fix: `IServiceScopeFactory` per iteration |
| `CpuYieldingService` | `BackgroundService` | Gotcha 5 fix: `Task.Yield()` between chunks for thread-pool fairness |

`Program.cs` also wires up Gotcha 1's fix: `BackgroundServiceExceptionBehavior.Ignore` so a transient error in any service does not crash the entire host.

## Run it

```bash
cd HostedServices.Api
dotnet run
```

You should see startup logs from `StartupTask`, then heartbeat logs from both heartbeat services every 30 seconds, then order-processed logs after about 15 seconds, then a prime-count log after about 1 minute.

Open Scalar at `http://localhost:5xxx/scalar/v1` (port shown in startup logs) to hit the `/orders` endpoint and confirm the BackgroundService has marked the seeded orders as processed.

Ctrl+C to stop. Every loop honors `stoppingToken`, so shutdown is immediate (no 30-second wait).

## The 5 production gotchas this sample fixes

1. **Unhandled exception crashes the host (.NET 6+)** — `Program.cs` sets `BackgroundServiceExceptionBehavior = Ignore`; each service wraps the iteration body in try/catch so transient errors do not propagate.
2. **Captive dependency on scoped services** — `OrderProcessor` injects `IServiceScopeFactory`, not `DbContext`, and creates a fresh scope per iteration.
3. **ExecuteAsync returning early silently kills the service** — every service logs on entry AND in a `finally` block on exit, so a silent death shows up in logs immediately.
4. **Ignoring `stoppingToken` abandons in-flight work on shutdown** — every `await` is passed the token; shutdown is instant instead of waiting the 5-second `HostOptions.ShutdownTimeout`.
5. **CPU-bound work starves the thread pool** — `CpuYieldingService` yields between chunks so other thread-pool consumers can run.

## Project layout

```
HostedServices.slnx
HostedServices.Api/
├── Program.cs                  # Host wiring + Gotcha 1 fix
├── HostedServices.Api.csproj
├── Data/
│   └── OrdersDbContext.cs      # EF Core 10 in-memory
└── Services/
    ├── StartupTask.cs
    ├── HeartbeatHostedService.cs
    ├── HeartbeatBackgroundService.cs
    ├── OrderProcessor.cs
    └── CpuYieldingService.cs
```

## Stack

- .NET 10 SDK
- ASP.NET Core 10
- Microsoft.AspNetCore.OpenApi 10.0.0
- Scalar.AspNetCore 2.13.18
- Microsoft.EntityFrameworkCore 10.0.0 (InMemory provider)

No Docker, no external services — runs anywhere `dotnet` is installed.
