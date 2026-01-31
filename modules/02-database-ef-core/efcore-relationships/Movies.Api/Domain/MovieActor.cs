namespace Movies.Api.Domain;

/// <summary>
/// Join entity for Many-to-Many relationship between Movies and Actors
/// with additional payload data (Role, CharacterName, BillingOrder)
/// </summary>
public class MovieActor
{
    public Guid MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    
    public Guid ActorId { get; set; }
    public Actor Actor { get; set; } = null!;
    
    // Payload - extra data about the relationship
    public string Role { get; set; } = string.Empty;  // "Lead", "Supporting", "Cameo"
    public string CharacterName { get; set; } = string.Empty;
    public int BillingOrder { get; set; }
}
