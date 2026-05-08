namespace ApiKeyAuth.Api.Validation;

public sealed record ApiKeyValidationResult(
    bool IsValid,
    string? Reason,
    Guid? KeyId,
    string? Name,
    string? OwnerId,
    string? Prefix,
    string[] Scopes)
{
    public static ApiKeyValidationResult Invalid(string reason, string? prefix = null) =>
        new(false, reason, null, null, null, prefix, []);
}
