```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9278/25H2/2025Update/HudsonValley2)
Intel Core Ultra 9 275HX 2.70GHz, 1 CPU, 24 logical and 24 physical cores
.NET SDK 10.0.303
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3


```
| Method                | Mean          | Error      | StdDev      | Ratio    | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |--------------:|-----------:|------------:|---------:|--------:|-------:|----------:|------------:|
| &#39;Result, depth 1&#39;     |     0.7262 ns |  0.0449 ns |   0.0798 ns |     1.01 |    0.14 |      - |         - |          NA |
| &#39;Result, depth 10&#39;    |     8.7811 ns |  0.1722 ns |   0.1345 ns |    12.21 |    1.11 |      - |         - |          NA |
| &#39;Exception, depth 1&#39;  | 1,113.2473 ns |  7.1101 ns |   6.3029 ns | 1,547.96 |  139.51 | 0.0153 |     320 B |          NA |
| &#39;Exception, depth 10&#39; | 3,670.8490 ns | 69.0903 ns | 109.5846 ns | 5,104.27 |  483.05 | 0.0687 |    1336 B |          NA |
| &#39;Result, success&#39;     |     1.0900 ns |  0.0154 ns |   0.0136 ns |     1.52 |    0.14 |      - |         - |          NA |
| &#39;Try/catch, no throw&#39; |     0.8315 ns |  0.0138 ns |   0.0123 ns |     1.16 |    0.11 |      - |         - |          NA |
