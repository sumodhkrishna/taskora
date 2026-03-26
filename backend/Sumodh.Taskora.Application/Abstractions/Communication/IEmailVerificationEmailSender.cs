namespace Sumodh.Taskora.Application.Abstractions.Communication
{
    public interface IEmailVerificationEmailSender
    {
        Task SendAsync(string name, string email, string verificationToken, CancellationToken cancellationToken);
    }
}
