namespace RefreshTokens.Api.Entities;

// One row per refresh token we hand out. We store these so we can rotate,
// expire, and revoke them - something we cannot do with a stateless JWT.
public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime Expires { get; set; }
    public DateTime? Revoked { get; set; }

    // When this token is rotated, we record the token that replaced it.
    public string? ReplacedByToken { get; set; }

    // A token is only usable if it has not been revoked and has not expired.
    public bool IsActive => Revoked is null && DateTime.UtcNow < Expires;
}
