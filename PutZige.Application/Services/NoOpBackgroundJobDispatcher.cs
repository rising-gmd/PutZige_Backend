using PutZige.Application.Interfaces;

namespace PutZige.Application.Services;

internal class NoOpBackgroundJobDispatcher : IBackgroundJobDispatcher
{
    public void EnqueueVerificationEmail(string email, string username, string token)
    {
        // no-op for unit tests and when DI not providing dispatcher
    }
}
