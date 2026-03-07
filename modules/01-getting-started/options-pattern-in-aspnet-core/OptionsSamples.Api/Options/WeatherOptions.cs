using System.ComponentModel.DataAnnotations;

namespace OptionsSamples.Api.Options;

public class WeatherOptions
{
    public const string SectionName = "WeatherOptions";

    [Required(AllowEmptyStrings = false)]
    public string City { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string State { get; set; } = string.Empty;

    [Range(0, 100)]
    public int Temperature { get; set; }

    public string? Summary { get; set; }
}
