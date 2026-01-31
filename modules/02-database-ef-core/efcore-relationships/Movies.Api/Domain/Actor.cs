namespace Movies.Api.Domain;

public class Actor : EntityBase
{
    public string Name { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }
    
    // Navigation - many actors in many movies
    public ICollection<Movie> Movies { get; private set; } = new List<Movie>();
    
    private Actor() { }
    
    private Actor(string name, DateOnly birthDate)
    {
        Name = name;
        BirthDate = birthDate;
    }
    
    public static Actor Create(string name, DateOnly birthDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Actor name cannot be empty.", nameof(name));
            
        return new Actor(name, birthDate);
    }
    
    public void Update(string name, DateOnly birthDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Actor name cannot be empty.", nameof(name));
            
        Name = name;
        BirthDate = birthDate;
        UpdateLastModified();
    }
}
