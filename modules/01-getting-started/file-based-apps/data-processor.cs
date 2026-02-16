#:package CsvHelper@33.0.0

using System.Text.Json;
using System.Text.Json.Serialization;
using CsvHelper;
using System.Globalization;

// Read JSON using source-generated serializer (AOT-compatible)
var json = await File.ReadAllTextAsync("sales.json");
var sales = JsonSerializer.Deserialize(json, SaleJsonContext.Default.ListSale)!;

// Process and write CSV
var summary = sales
    .GroupBy(s => s.Product)
    .Select(g => new SaleSummary(g.Key, g.Sum(s => s.Amount), g.Count()))
    .OrderByDescending(x => x.TotalRevenue);

using var writer = new StreamWriter("summary.csv");
using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
csv.WriteRecords(summary);

Console.WriteLine("Done! Check summary.csv");

record Sale(string Product, decimal Amount, DateTime Date);
record SaleSummary(string Product, decimal TotalRevenue, int Count);

[JsonSerializable(typeof(List<Sale>))]
partial class SaleJsonContext : JsonSerializerContext { }
