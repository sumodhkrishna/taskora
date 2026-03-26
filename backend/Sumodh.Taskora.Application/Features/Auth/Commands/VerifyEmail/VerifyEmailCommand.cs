namespace Sumodh.Taskora.Application.Features.Auth.Commands.VerifyEmail
{
    public sealed record VerifyEmailCommand(string Email, string Token);
}
