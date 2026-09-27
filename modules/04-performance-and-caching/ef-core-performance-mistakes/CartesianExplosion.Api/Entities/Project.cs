namespace CartesianExplosion.Api.Entities;

public sealed class Project
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public required string Name { get; set; }
    public required string Summary { get; set; }
}
