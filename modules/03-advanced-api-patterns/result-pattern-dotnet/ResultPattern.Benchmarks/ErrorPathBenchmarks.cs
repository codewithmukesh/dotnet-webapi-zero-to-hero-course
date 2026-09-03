using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace ResultPattern.Benchmarks;

/// <summary>
/// Compares the cost of signalling an expected failure with a Result type
/// versus throwing an exception, at two different call-stack depths.
/// Depth is the number of frames between the failure and the handler.
/// </summary>
[MemoryDiagnoser]
public class ErrorPathBenchmarks
{
    private const int Depth = 10;

    // ---------- Result path ----------

    [Benchmark(Baseline = true, Description = "Result, depth 1")]
    public string ResultDepth1()
    {
        var result = ResultLeaf();
        return result.IsSuccess ? result.Value! : result.Error!;
    }

    [Benchmark(Description = "Result, depth 10")]
    public string ResultDepth10()
    {
        var result = ResultRecurse(Depth);
        return result.IsSuccess ? result.Value! : result.Error!;
    }

    // ---------- Exception path ----------

    [Benchmark(Description = "Exception, depth 1")]
    public string ExceptionDepth1()
    {
        try
        {
            return ThrowLeaf();
        }
        catch (OrderNotFoundException ex)
        {
            return ex.Message;
        }
    }

    [Benchmark(Description = "Exception, depth 10")]
    public string ExceptionDepth10()
    {
        try
        {
            return ThrowRecurse(Depth);
        }
        catch (OrderNotFoundException ex)
        {
            return ex.Message;
        }
    }

    // ---------- Success path, for reference ----------

    [Benchmark(Description = "Result, success")]
    public string ResultSuccess()
    {
        var result = ResultSuccessLeaf();
        return result.IsSuccess ? result.Value! : result.Error!;
    }

    [Benchmark(Description = "Try/catch, no throw")]
    public string TryCatchNoThrow()
    {
        try
        {
            return SuccessLeaf();
        }
        catch (OrderNotFoundException ex)
        {
            return ex.Message;
        }
    }

    // ---------- Helpers ----------

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Result<string> ResultLeaf() => Result<string>.Failure("Order 42 was not found.");

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Result<string> ResultSuccessLeaf() => Result<string>.Success("Order 42");

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Result<string> ResultRecurse(int depth) =>
        depth <= 0 ? ResultLeaf() : ResultRecurse(depth - 1);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string ThrowLeaf() => throw new OrderNotFoundException("Order 42 was not found.");

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string SuccessLeaf() => "Order 42";

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string ThrowRecurse(int depth) =>
        depth <= 0 ? ThrowLeaf() : ThrowRecurse(depth - 1);
}

public sealed class OrderNotFoundException(string message) : Exception(message);

public readonly record struct Result<T>
{
    private Result(T? value, string? error, bool isSuccess)
    {
        Value = value;
        Error = error;
        IsSuccess = isSuccess;
    }

    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess { get; }

    public static Result<T> Success(T value) => new(value, null, true);
    public static Result<T> Failure(string error) => new(default, error, false);
}
