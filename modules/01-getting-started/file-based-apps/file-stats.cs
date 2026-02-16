#:package System.CommandLine@2.0.0

using System.CommandLine;

var fileOption = new Option<FileInfo?>("--file") { Description = "The file to process" };
var verboseOption = new Option<bool>("--verbose") { Description = "Show detailed output" };

var rootCommand = new RootCommand("File statistics utility - counts lines, words, and characters")
{
    fileOption,
    verboseOption
};

rootCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var file = parseResult.GetValue(fileOption);
    var verbose = parseResult.GetValue(verboseOption);

    if (file is null)
    {
        Console.WriteLine("No file specified. Use --file <path>");
        return 1;
    }

    if (!file.Exists)
    {
        Console.WriteLine($"File not found: {file.FullName}");
        return 1;
    }

    if (verbose)
        Console.WriteLine($"Processing: {file.FullName}");

    var lines = await File.ReadAllLinesAsync(file.FullName, cancellationToken);
    Console.WriteLine($"Lines: {lines.Length}");
    Console.WriteLine($"Words: {lines.Sum(l => l.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length)}");
    Console.WriteLine($"Characters: {lines.Sum(l => l.Length)}");

    return 0;
});

return await rootCommand.Parse(args).InvokeAsync();
