using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class EmailService : IEmailService, IScopedService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SIMULATED EMAIL SENT TO: {To}", to);
        _logger.LogInformation("SUBJECT: {Subject}", subject);
        _logger.LogInformation("BODY: {Body}", body);
        
        return Task.CompletedTask;
    }
}
