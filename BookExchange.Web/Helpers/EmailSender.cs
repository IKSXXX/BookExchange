using Microsoft.AspNetCore.Identity.UI.Services;

namespace BookExchange.Web.Helpers;

/// <summary>
/// Заглушка отправки e-mail: печатает в лог вместо реальной отправки.
/// Реальная реализация — через SmtpClient/SendGrid/Mailgun и т.п.
/// </summary>
public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;
    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) => _logger = logger;

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("[EMAIL STUB] To: {email} | Subject: {subject} | Body: {body}", email, subject, htmlMessage);
        return Task.CompletedTask;
    }
}
