using PutZige.Application.Interfaces;

namespace PutZige.Infrastructure.Services;

public class HangfireBackgroundJobDispatcher : IBackgroundJobDispatcher
{
    public void EnqueueVerificationEmail(string email, string username, string token)
    {
        Hangfire.BackgroundJob.Enqueue<EmailBackgroundService>(x => x.SendVerificationEmailJob(email, username, token));
    }
}
