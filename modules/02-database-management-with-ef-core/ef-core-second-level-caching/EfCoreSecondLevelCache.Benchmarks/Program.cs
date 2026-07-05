using BenchmarkDotNet.Running;
using EfCoreSecondLevelCache.Benchmarks;

BenchmarkRunner.Run<CacheBenchmarks>();
