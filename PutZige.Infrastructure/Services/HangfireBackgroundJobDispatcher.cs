using PutZige.Application.Interfaces;
using Hangfire;

namespace PutZige.Infrastructure.Services;

public class HangfireBackgroundJobDispatcher : IBackgroundJobDispatcher
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireBackgroundJobDispatcher(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient ?? throw new System.ArgumentNullException(nameof(backgroundJobClient));
    }

    public void EnqueueVerificationEmail(string email, string username, string token)
    {
        _backgroundJobClient.Enqueue<EmailBackgroundService>(x => x.SendVerificationEmailJob(email, username, token));
    }
}
