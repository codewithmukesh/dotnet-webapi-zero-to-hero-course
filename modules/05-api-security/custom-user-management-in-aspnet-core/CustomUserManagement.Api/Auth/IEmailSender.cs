namespace CustomUserManagement.Api.Auth;

// The seam for email confirmation and password reset. In production you implement this
// with SendGrid, Amazon SES, SMTP, etc. Here we log the message so the demo runs with
// zero setup and you can copy the token straight out of the console.
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}

public class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string body)
    {
        logger.LogInformation("EMAIL -> {To}\nSubject: {Subject}\n{Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
