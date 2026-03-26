namespace Sumodh.Taskora.Application.Abstractions.Communication
{
    public interface IPasswordResetEmailSender
    {
        Task SendAsync(string name, string email, string resetToken, CancellationToken cancellationToken);
    }
}
