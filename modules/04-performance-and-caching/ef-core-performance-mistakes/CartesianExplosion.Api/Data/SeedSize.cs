using System.Globalization;

namespace CartesianExplosion.Api.Data;

/// <summary>How much data to seed, and the row math the video quotes.</summary>
public sealed record SeedSize(int Departments, int ProjectsPerDepartment, int EmployeesPerDepartment, int DocumentsPerDepartment)
{
    /// <summary>Engineering with Aryabhata, Bhaskara, Chandrayaan and Aarav, Bhavya, Chetan, Divya (plus 2 documents).</summary>
    public static SeedSize Tiny { get; } = new(1, 3, 4, 2);

    /// <summary>The article's example: 50 departments, 20 projects and 30 employees each (plus 10 documents).</summary>
    public static SeedSize Article { get; } = new(50, 20, 30, 10);

    public static SeedSize Parse(string value) => value switch
    {
        "Tiny" => Tiny,
        "Article" => Article,
        _ => ParseCustom(value),
    };

    /// <summary>
    /// Rows a single query returns. Sibling collections are LEFT JOINed, so their counts multiply.
    /// An empty collection still yields one row (the LEFT JOIN keeps the parent), hence Max(1, n).
    /// </summary>
    public long SingleQueryRows(bool withDocuments) =>
        (long)Departments
        * Math.Max(1, ProjectsPerDepartment)
        * Math.Max(1, EmployeesPerDepartment)
        * (withDocuments ? Math.Max(1, DocumentsPerDepartment) : 1);

    /// <summary>Records actually needed; also the total rows a split query returns.</summary>
    public long RecordCount(bool withDocuments) =>
        Departments
        + (long)Departments * ProjectsPerDepartment
        + (long)Departments * EmployeesPerDepartment
        + (withDocuments ? (long)Departments * DocumentsPerDepartment : 0);

    private static SeedSize ParseCustom(string value)
    {
        var parts = value.Split('x');
        if (parts.Length != 4)
        {
            throw new FormatException($"Seed size '{value}' must be Tiny, Article or DxPxExD (for example 50x20x30x10).");
        }

        var n = parts.Select(p => int.Parse(p, CultureInfo.InvariantCulture)).ToArray();
        return new SeedSize(n[0], n[1], n[2], n[3]);
    }
}
