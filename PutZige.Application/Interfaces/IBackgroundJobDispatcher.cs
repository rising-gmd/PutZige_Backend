namespace PutZige.Application.Interfaces;

public interface IBackgroundJobDispatcher
{
    void EnqueueVerificationEmail(string email, string username, string token);
}
