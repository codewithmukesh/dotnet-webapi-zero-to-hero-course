namespace Movies.Api.Domain;

public class Director : EntityBase
{
    public string Name { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }
    
    // Navigation property - one director has many movies
    public ICollection<Movie> Movies { get; private set; } = new List<Movie>();
    
    // Private constructor for EF Core
    private Director() { }
    
    private Director(string name, string country, DateOnly birthDate)
    {
        Name = name;
        Country = country;
        BirthDate = birthDate;
    }
    
    public static Director Create(string name, string country, DateOnly birthDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Director name cannot be empty.", nameof(name));
            
        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));
            
        return new Director(name, country, birthDate);
    }
    
    public void Update(string name, string country, DateOnly birthDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Director name cannot be empty.", nameof(name));
            
        Name = name;
        Country = country;
        BirthDate = birthDate;
        UpdateLastModified();
    }
}
