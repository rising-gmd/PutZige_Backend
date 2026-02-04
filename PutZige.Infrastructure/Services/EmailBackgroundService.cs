using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using PutZige.Application.Interfaces;

namespace PutZige.Infrastructure.Services;

public class EmailBackgroundService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailBackgroundService> _logger;

    public EmailBackgroundService(IEmailService emailService, ILogger<EmailBackgroundService> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task SendVerificationEmailJob(string email, string username, string token)
    {
        try
        {
            await _emailService.SendVerificationEmailAsync(email, username, token).ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Background job failed to send verification email to {email}", email);
            throw;
        }
    }
}
