namespace PermissionBasedAuth.Api.Auth;

// Where the authorization handler reads a caller's permissions from.
public enum PermissionSource
{
    // Permissions are stamped into the JWT at login and read back off the
    // ClaimsPrincipal. Zero lookup cost, but the token grows with every
    // permission and stays stale until it expires.
    Token,

    // Only roles travel in the token. Permissions are resolved per request
    // from the store. Small tokens, instant revocation, one cached lookup.
    Lookup
}

public class PermissionSettings
{
    public PermissionSource Source { get; set; } = PermissionSource.Lookup;
}
