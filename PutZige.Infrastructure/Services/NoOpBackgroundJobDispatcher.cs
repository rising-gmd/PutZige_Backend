using PutZige.Application.Interfaces;

namespace PutZige.Infrastructure.Services;

internal class NoOpBackgroundJobDispatcher : IBackgroundJobDispatcher
{
    public void EnqueueVerificationEmail(string email, string username, string token)
    {
        // Intentionally no-op when Hangfire is not configured.
    }
}
