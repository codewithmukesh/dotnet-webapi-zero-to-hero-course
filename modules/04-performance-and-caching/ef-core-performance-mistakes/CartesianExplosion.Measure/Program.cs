using CartesianExplosion.Measure;

var command = args.FirstOrDefault() ?? "help";
var options = Options.Parse(args.Skip(1));

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

return command switch
{
    "bench" => await Bench.RunAsync(options, cts.Token),
    "growth" => await Growth.RunAsync(options, cts.Token),
    "memory" => await Memory.RunAsync(options, cts.Token),
    "load" => await Load.RunAsync(options, cts.Token),
    _ => Help(),
};

static int Help()
{
    Console.WriteLine("usage: bench | growth | memory | load   [--key value] ...");
    return 1;
}
