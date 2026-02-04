using System.Threading;
using System.Threading.Tasks;

namespace PutZige.Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string username, string verificationToken, CancellationToken ct = default);
    Task SendPasswordResetEmailAsync(string toEmail, string username, string resetToken, CancellationToken ct = default);
}
