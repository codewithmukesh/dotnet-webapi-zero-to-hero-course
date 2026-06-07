namespace RefreshTokens.Api.Auth;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    // Access tokens are short-lived; refresh tokens live much longer.
    public int AccessTokenMinutes { get; set; }
    public int RefreshTokenDays { get; set; }
}
