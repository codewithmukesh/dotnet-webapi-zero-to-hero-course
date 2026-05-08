namespace ApiKeyAuth.Api.Validation;

public interface IApiKeyValidator
{
    Task<ApiKeyValidationResult> ValidateAsync(string plaintextKey, CancellationToken ct);
}
