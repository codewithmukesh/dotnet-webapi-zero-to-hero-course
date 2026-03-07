using Microsoft.Extensions.Options;
using OptionsSamples.Api.Options;

namespace OptionsSamples.Api.Validation;

public class WeatherOptionsValidator : IValidateOptions<WeatherOptions>
{
    public ValidateOptionsResult Validate(string? name, WeatherOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.City))
        {
            failures.Add("City is required and cannot be empty.");
        }

        if (options.Temperature is < 0 or > 100)
        {
            failures.Add($"Temperature must be between 0 and 100. Got: {options.Temperature}.");
        }

        if (!string.IsNullOrWhiteSpace(options.Summary) && options.Summary.Length > 200)
        {
            failures.Add("Summary must be 200 characters or fewer.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
