namespace CustomJwt.Api.Auth;

// Strongly-typed version of the "JwtSettings" section in appsettings.json.
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
}
