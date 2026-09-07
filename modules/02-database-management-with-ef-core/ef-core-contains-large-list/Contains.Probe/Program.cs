using System.Text;
using EfCoreContainsLargeList.Probe;
using EfCoreContainsLargeList.Shared;
using Microsoft.EntityFrameworkCore;

// Probes what EF Core 10 actually sends to SQL Server for a parameterized Contains,
// across list sizes, translation modes, and the parameter-limit boundary.

// "--composite <size>": what EF Core 10 does when the filter list is composite keys.
// "--composite-one <approach> <size>": one approach, one process. Used as a child process
// by --composite and --composite-limit, because a stack overflow cannot be caught.
if (args is ["--composite-one", var approachArg, var oneSizeArg])
{
    await CompositeProbe.RunOneAsync(approachArg, int.Parse(oneSizeArg));
    return;
}

// "--composite-limit <approach> <low> <high>": bisects the largest list an approach survives.
if (args is ["--composite-limit", var limitApproach, var lowArg, var highArg])
{
    await CompositeProbe.FindOrChainLimitAsync(limitApproach, int.Parse(lowArg), int.Parse(highArg));
    return;
}

if (args is ["--composite", .. var pairArgs] && pairArgs.Length > 0)
{
    foreach (var pairArg in pairArgs)
    {
        await CompositeProbe.RunAsync(int.Parse(pairArg));
    }

    return;
}

await DbSetup.EnsureSeededAsync();

// Single-case mode: "--single <mode|default> <size>".
// EF caches compiled queries per model, so probing several global modes inside one
// process can hand you the first mode's SQL for every later run. Each global mode
// therefore gets its own process.
if (args is ["--single", var modeArg, var sizeArg])
{
    ParameterTranslationMode? single = modeArg switch
    {
        "MultipleParameters" => ParameterTranslationMode.MultipleParameters,
        "Parameter" => ParameterTranslationMode.Parameter,
        "Constant" => ParameterTranslationMode.Constant,
        _ => null
    };

    var singleResult = await RunAsync(int.Parse(sizeArg), single);
    Console.WriteLine(
        $"{modeArg,-20} size={sizeArg,-7} params={singleResult.ParameterCount,-7} {singleResult.Shape}");
    return;
}

// "--plancache <mode>": how many distinct plans 20 differently-sized lists leave behind.
if (args is ["--plancache", var planMode])
{
    ParameterTranslationMode? mode = planMode switch
    {
        "MultipleParameters" => ParameterTranslationMode.MultipleParameters,
        "Parameter" => ParameterTranslationMode.Parameter,
        "Constant" => ParameterTranslationMode.Constant,
        _ => null
    };

    await using (var reset = new AppDbContext())
    {
        await reset.Database.ExecuteSqlRawAsync("DBCC FREEPROCCACHE WITH NO_INFOMSGS;");
    }

    await using var ctx = new AppDbContext(mode);
    for (var size = 100; size < 120; size++)
    {
        var ids = DbSetup.BuildIds(size);
        _ = await ctx.Products.AsNoTracking().Where(p => ids.Contains(p.Id)).Select(p => p.Id).CountAsync();
    }

    await using var count = new AppDbContext();
    var plans = await count.Database
        .SqlQueryRaw<int>(
            """
            SELECT COUNT(*) AS [Value]
            FROM sys.dm_exec_cached_plans cp
            CROSS APPLY sys.dm_exec_sql_text(cp.plan_handle) t
            WHERE t.text LIKE '%[Products]%' AND t.text NOT LIKE '%dm_exec_cached_plans%'
            """)
        .SingleAsync();

    Console.WriteLine($"{planMode,-20} 20 distinct list sizes -> {plans} cached plans");
    return;
}

var report = new StringBuilder();
var efVersion = typeof(DbContext).Assembly.GetName().Version?.ToString() ?? "unknown";

report.AppendLine("# What EF Core actually sends");
report.AppendLine();
report.AppendLine($"- EF Core assembly version: `{efVersion}`");
report.AppendLine($"- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
report.AppendLine();

await PaddingProbeAsync(report);
await BoundaryProbeAsync(report);
await ModeProbeAsync(report);
await RedactionProbeAsync(report);

var outputPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "probe-output.md");
outputPath = Path.GetFullPath(outputPath);
await File.WriteAllTextAsync(outputPath, report.ToString());

Console.WriteLine();
Console.WriteLine($"Report written to {outputPath}");

// ---------------------------------------------------------------------------

static async Task PaddingProbeAsync(StringBuilder report)
{
    Console.WriteLine();
    Console.WriteLine("== Padding probe ==");

    report.AppendLine("## Padding: list size vs parameters actually sent");
    report.AppendLine();
    report.AppendLine("| List size | Parameters sent | Padding added | Translation |");
    report.AppendLine("|---|---|---|---|");

    int[] sizes = [1, 2, 3, 5, 6, 8, 10, 11, 20, 50, 100, 150, 151, 200, 500, 751, 800, 810, 1000, 1010, 1500, 1501, 1990, 2000, 2001, 2050, 2069, 2090, 2091, 2094, 2098];

    foreach (var size in sizes)
    {
        var result = await RunAsync(size);
        var padding = result.ParameterCount - size;
        report.AppendLine(
            $"| {size:N0} | {result.ParameterCount:N0} | {(padding >= 0 ? "+" + padding : padding.ToString())} | {result.Shape} |");
        Console.WriteLine($"  {size,7:N0} -> {result.ParameterCount,7:N0} params  {result.Shape}");
    }

    report.AppendLine();
}

static async Task BoundaryProbeAsync(StringBuilder report)
{
    Console.WriteLine();
    Console.WriteLine("== Boundary probe (the 2,098 ceiling) ==");

    report.AppendLine("## The boundary: what happens as the list crosses the parameter ceiling");
    report.AppendLine();
    report.AppendLine("| List size | Outcome | Parameters sent | Translation |");
    report.AppendLine("|---|---|---|---|");

    int[] sizes =
    [
        2000, 2050, 2090, 2094, 2095, 2096, 2097, 2098, 2099, 2100,
        2101, 2200, 2500, 5000, 10_000, 50_000, 100_000
    ];

    foreach (var size in sizes)
    {
        var result = await RunAsync(size);
        var outcome = result.Error is null ? $"OK ({result.RowsReturned:N0} rows)" : $"**{result.Error}**";
        report.AppendLine($"| {size:N0} | {outcome} | {result.ParameterCount:N0} | {result.Shape} |");
        Console.WriteLine($"  {size,7:N0} -> {outcome,-24} {result.ParameterCount,7:N0} params  {result.Shape}");
    }

    report.AppendLine();
}

static async Task ModeProbeAsync(StringBuilder report)
{
    Console.WriteLine();
    Console.WriteLine("== Translation mode probe ==");

    (string Label, ParameterTranslationMode? Mode, bool UseParameterHelper, bool UseConstantHelper)[] cases =
    [
        ("default (MultipleParameters)", null, false, false),
        ("global: MultipleParameters", ParameterTranslationMode.MultipleParameters, false, false),
        ("global: Parameter", ParameterTranslationMode.Parameter, false, false),
        ("global: Constant", ParameterTranslationMode.Constant, false, false),
        ("per-query: EF.Parameter", null, true, false),
        ("per-query: EF.Constant", null, false, true),
        ("per-query: EF.MultipleParameters", null, false, false)
    ];

    _ = cases;

    // Below the ceiling the mode is honoured as configured; above it, the over-limit
    // handler can override the choice. Probing both sizes separates the two effects.
    //
    // Global modes are measured in a child process each. EF caches compiled queries per
    // model, so running several global modes in one process returns the FIRST mode's SQL
    // for every later run - a trap worth knowing if you write your own probe.
    foreach (var size in (int[])[100, 5_000])
    {
        Console.WriteLine($"  -- {size:N0}-item list --");
        report.AppendLine($"## Translation modes at a {size:N0}-item list");
        report.AppendLine();
        report.AppendLine("| Mode | Parameters sent | Translation |");
        report.AppendLine("|---|---|---|");

        foreach (var mode in (string[])["default", "MultipleParameters", "Parameter", "Constant"])
        {
            var line = await RunInChildProcessAsync(mode, size);
            report.AppendLine(line.Row);
            Console.WriteLine($"    {line.Console}");
        }

        foreach (var (label, useParameter, useConstant) in
                 ((string, bool, bool)[])[("per-query: EF.Parameter", true, false), ("per-query: EF.Constant", false, true)])
        {
            var result = await RunAsync(size, null, useParameter, useConstant);
            report.AppendLine($"| {label} | {result.ParameterCount:N0} | {result.Shape} |");
            Console.WriteLine($"    {label,-32} {result.ParameterCount,7:N0} params  {result.Shape}");
        }

        report.AppendLine();
    }

    await SampleSqlAsync(report);
}

static async Task<(string Row, string Console)> RunInChildProcessAsync(string mode, int size)
{
    var info = new System.Diagnostics.ProcessStartInfo
    {
        FileName = Environment.ProcessPath!,
        RedirectStandardOutput = true,
        UseShellExecute = false
    };

    // When published as a framework-dependent apphost, ProcessPath is the exe itself.
    info.ArgumentList.Add("--single");
    info.ArgumentList.Add(mode);
    info.ArgumentList.Add(size.ToString());

    using var process = System.Diagnostics.Process.Start(info)!;
    var stdout = await process.StandardOutput.ReadToEndAsync();
    await process.WaitForExitAsync();

    var line = stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .LastOrDefault(l => l.Contains("params="))?.Trim() ?? "(no output)";

    var parameters = Between(line, "params=", " ");
    var shape = line.Contains("OPENJSON") ? "single JSON parameter (`OPENJSON`)"
        : line.Contains("inlined") ? "inlined constants"
        : "multiple scalar parameters";

    var label = mode == "default" ? "default (MultipleParameters)" : $"global: {mode}";
    return ($"| {label} | {parameters} | {shape} |", $"{label,-32} {parameters,7} params  {shape}");
}

static string Between(string source, string start, string end)
{
    var from = source.IndexOf(start, StringComparison.Ordinal);
    if (from < 0)
    {
        return "?";
    }

    from += start.Length;
    var to = source.IndexOf(end, from, StringComparison.Ordinal);
    return (to < 0 ? source[from..] : source[from..to]).Trim();
}

static async Task SampleSqlAsync(StringBuilder report)
{
    report.AppendLine("## Sample generated SQL");
    report.AppendLine();

    // Every sample below varies the LINQ expression rather than a global option, so each
    // one gets its own compiled-query cache entry and the captured SQL is genuinely its own.
    (string Label, int Size, bool Parameter, bool Constant)[] samples =
    [
        ("3 items, default", 3, false, false),
        ("8 items, default (padding visible)", 8, false, false),
        ("3 items, EF.Parameter", 3, true, false),
        ("3 items, EF.Constant", 3, false, true),
        ("2,098 items, default (last multi-parameter size)", 2_098, false, false),
        ("2,099 items, default (first fallback size)", 2_099, false, false)
    ];

    foreach (var (label, size, useParameter, useConstant) in samples)
    {
        var capture = new SqlCaptureInterceptor();
        await using var context = new AppDbContext(capture: capture);
        var ids = DbSetup.BuildIds(size);
        var q = context.Products.AsNoTracking();
        _ = useParameter
            ? await q.Where(p => EF.Parameter(ids).Contains(p.Id)).Select(p => p.Id).CountAsync()
            : useConstant
                ? await q.Where(p => EF.Constant(ids).Contains(p.Id)).Select(p => p.Id).CountAsync()
                : await q.Where(p => ids.Contains(p.Id)).Select(p => p.Id).CountAsync();

        var sql = capture.LastSql ?? "(nothing captured)";
        if (sql.Length > 700)
        {
            sql = sql[..350] + "\n    ...\n" + sql[^250..];
        }

        report.AppendLine($"### {label}");
        report.AppendLine();
        report.AppendLine($"Parameters sent: **{capture.LastParameterCount:N0}**");
        report.AppendLine();
        report.AppendLine("```sql");
        report.AppendLine(sql);
        report.AppendLine("```");
        report.AppendLine();
    }
}

static async Task RedactionProbeAsync(StringBuilder report)
{
    Console.WriteLine();
    Console.WriteLine("== EF.Constant log redaction probe ==");

    report.AppendLine("## EF.Constant: what runs vs what gets logged");
    report.AppendLine();

    var capture = new SqlCaptureInterceptor();
    await using var context = new AppDbContext(capture: capture);
    var ids = DbSetup.BuildIds(3);

    _ = await context.Products
        .AsNoTracking()
        .Where(p => EF.Constant(ids).Contains(p.Id))
        .Select(p => p.Id)
        .ToListAsync();

    var executed = ExtractPredicate(capture.LastSql);
    report.AppendLine("Executed against the database (seen from a `DbCommandInterceptor`):");
    report.AppendLine();
    report.AppendLine("```sql");
    report.AppendLine(executed);
    report.AppendLine("```");
    report.AppendLine();
    report.AppendLine("EF Core 10 redacts these inlined values in its own logs unless `EnableSensitiveDataLogging()` is on.");
    report.AppendLine();

    Console.WriteLine($"  executed predicate: {executed}");
}

static async Task<ProbeResult> RunAsync(
    int size,
    ParameterTranslationMode? mode = null,
    bool useParameterHelper = false,
    bool useConstantHelper = false)
{
    var capture = new SqlCaptureInterceptor();
    await using var context = new AppDbContext(mode, capture);
    var ids = DbSetup.BuildIds(size);

    try
    {
        var query = context.Products.AsNoTracking();

        var rows = useParameterHelper
            ? await query.Where(p => EF.Parameter(ids).Contains(p.Id)).Select(p => p.Id).CountAsync()
            : useConstantHelper
                ? await query.Where(p => EF.Constant(ids).Contains(p.Id)).Select(p => p.Id).CountAsync()
                : await query.Where(p => ids.Contains(p.Id)).Select(p => p.Id).CountAsync();

        return new ProbeResult(
            capture.LastParameterCount,
            Classify(capture.LastSql),
            capture.LastSql?.Length ?? 0,
            rows,
            null);
    }
    catch (Exception ex)
    {
        return new ProbeResult(
            capture.LastParameterCount,
            Classify(capture.LastSql),
            capture.LastSql?.Length ?? 0,
            0,
            ex.GetType().Name + ": " + FirstLine(ex.Message));
    }
}

static string Classify(string? sql)
{
    if (string.IsNullOrEmpty(sql))
    {
        return "no command reached the database";
    }

    if (sql.Contains("OPENJSON", StringComparison.OrdinalIgnoreCase))
    {
        return "single JSON parameter (`OPENJSON`)";
    }

    if (sql.Contains("@ids", StringComparison.OrdinalIgnoreCase))
    {
        return "multiple scalar parameters";
    }

    return "inlined constants";
}

static string ExtractPredicate(string? sql)
{
    if (string.IsNullOrEmpty(sql))
    {
        return "(nothing captured)";
    }

    var index = sql.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase);
    var slice = index >= 0 ? sql[index..] : sql;
    return slice.Length > 300 ? slice[..300] + " ..." : slice;
}

static string FirstLine(string message)
{
    var line = message.Split('\n')[0].Trim();
    return line.Length > 120 ? line[..120] + "..." : line;
}

internal readonly record struct ProbeResult(
    int ParameterCount,
    string Shape,
    int SqlLength,
    int RowsReturned,
    string? Error);
