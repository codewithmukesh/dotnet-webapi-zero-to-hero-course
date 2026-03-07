using System.ComponentModel.DataAnnotations;

namespace OptionsSamples.Api.Options;

public class NotificationOptions
{
    public const string SectionName = "NotificationOptions";

    [Required(AllowEmptyStrings = false)]
    public string DefaultProvider { get; set; } = string.Empty;

    public EmailOptions Email { get; set; } = new();
    public SmsOptions Sms { get; set; } = new();
}

public class EmailOptions
{
    [Required, EmailAddress]
    public string From { get; set; } = string.Empty;

    [Required]
    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;
}

public class SmsOptions
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;
}
