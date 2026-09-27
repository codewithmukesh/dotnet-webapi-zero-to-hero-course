namespace CartesianExplosion.Api.Entities;

public sealed class Employee
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Title { get; set; }
}
