namespace Sumodh.Taskora.Application.Features.Auth.Commands.Register
{
    public sealed record RegisterCommand(
        string Name,
        string Email,
        string Password);
}
