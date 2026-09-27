using CartesianExplosion.Api.Entities;

namespace CartesianExplosion.Api.Data;

public static class Seeder
{
    private static readonly string[] ProjectNames = ["Aryabhata", "Bhaskara", "Chandrayaan"];
    private static readonly string[] EmployeeNames = ["Aarav", "Bhavya", "Chetan", "Divya"];
    private const string Filler = "Owns planning, delivery and support for its area, and reports progress at the end of every sprint.";

    /// <summary>Drops and recreates the database, then seeds a deterministic data set.</summary>
    public static async Task ResetAsync(AppDbContext db, SeedSize size, CancellationToken ct)
    {
        await db.Database.EnsureDeletedAsync(ct);
        await db.Database.EnsureCreatedAsync(ct);

        for (var d = 1; d <= size.Departments; d++)
        {
            db.Departments.Add(new Department
            {
                Name = d == 1 ? "Engineering" : $"Department {d:00}",
                Description = Filler,
                Projects = [.. Enumerable.Range(1, size.ProjectsPerDepartment).Select(i => new Project
                {
                    Name = Pick(ProjectNames, i, "Project"),
                    Summary = Filler,
                })],
                Employees = [.. Enumerable.Range(1, size.EmployeesPerDepartment).Select(i => new Employee
                {
                    FullName = Pick(EmployeeNames, i, "Employee"),
                    Email = $"employee{i:000}.department{d:00}@example.com",
                    Title = "Software Engineer",
                })],
                Documents = [.. Enumerable.Range(1, size.DocumentsPerDepartment).Select(i => new Document
                {
                    Title = $"Runbook {i:00}",
                    Path = $"/docs/department-{d:00}/runbook-{i:00}.pdf",
                })],
            });
        }

        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();
    }

    private static string Pick(string[] names, int i, string prefix) =>
        i <= names.Length ? names[i - 1] : $"{prefix} {i:000}";
}
