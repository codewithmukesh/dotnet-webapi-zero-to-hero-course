# Mapping Benchmarks

BenchmarkDotNet results for AutoMapper vs Mapster vs Mapperly vs Manual mapping on .NET 10.

## Setup

- **CPU**: Intel Core Ultra 9 275HX 2.70GHz, 24 logical / 24 physical cores
- **OS**: Windows 11 (10.0.26200.8457)
- **SDK**: .NET 10.0.203 / Runtime: .NET 10.0.8, X64 RyuJIT x86-64-v3
- **BenchmarkDotNet**: 0.15.4
- **Packages**: AutoMapper 14.0.0, Mapster 7.4.0, Riok.Mapperly 4.2.1

## Workload

One `Product -> ProductResponse` mapping per invocation. Each `Product` carries a nested `Category` and a `List<Tag>` of 3 items. Each mapper is pre-configured and pre-compiled at fixture construction, so the steady-state hot path is measured. Manual is the baseline.

## Results

| Method     | Mean     | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------|---------:|---------:|---------:|------:|-------:|----------:|------------:|
| Manual     | 39.73 ns | 1.579 ns | 4.605 ns |  1.01 | 0.0174 |     328 B |        1.00 |
| **Mapperly** | **31.48 ns** | **1.008 ns** | **2.892 ns** | **0.80** | **0.0157** | **296 B**     |        **0.90** |
| Mapster    | 50.42 ns | 1.502 ns | 4.260 ns |  1.29 | 0.0174 |     328 B |        1.00 |
| AutoMapper | 82.66 ns | 2.359 ns | 6.956 ns |  2.11 | 0.0178 |     336 B |        1.02 |

## Headlines

- **Mapperly is ~21% faster than Manual mapping** on this workload (31.5 ns vs 39.7 ns) and allocates ~10% less memory per call (296 B vs 328 B). The 32 B saving comes from avoiding the closure that `List<T>.ConvertAll(static t => ...)` produces in the manual code path - Mapperly's generated code emits a direct `for` loop instead.
- **AutoMapper is ~2.1x slower than Manual** (82.7 ns vs 39.7 ns) and allocates only ~2% more memory. The "AutoMapper is 10x slower" narrative is true only for specific workloads (deep object graphs, missing configuration cache, cold-start); on the hot path with a pre-built mapper it is closer to ~2x.
- **Mapster lands at ~1.29x of Manual** (50.4 ns vs 39.7 ns) - faster than AutoMapper but does not beat hand-written code.

## Reproduce

```bash
cd Mapping.Benchmarks
dotnet run -c Release
```
