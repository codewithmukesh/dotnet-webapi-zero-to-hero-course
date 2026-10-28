using System.ComponentModel.DataAnnotations;

namespace HttpClients.Api.Options;

public sealed class CatalogApiOptions
{
    public const string SectionName = "CatalogApi";

    [Required, Url]
    public string BaseAddress { get; init; } = string.Empty;

    // Keep the real key in user secrets or an environment variable, never in source control.
    [Required]
    public string ApiKey { get; init; } = string.Empty;
}
