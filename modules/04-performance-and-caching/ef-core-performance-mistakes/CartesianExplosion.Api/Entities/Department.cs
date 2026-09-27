namespace CartesianExplosion.Api.Entities;

public sealed class Department
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public List<Project> Projects { get; set; } = [];
    public List<Employee> Employees { get; set; } = [];
    public List<Document> Documents { get; set; } = [];
}
