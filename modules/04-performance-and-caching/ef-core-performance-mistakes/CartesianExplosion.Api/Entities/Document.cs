namespace CartesianExplosion.Api.Entities;

public sealed class Document
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public required string Title { get; set; }
    public required string Path { get; set; }
}
