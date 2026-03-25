namespace Sumodh.Taskora.Application.Abstractions.Identity
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        string? Email { get; }
        string? Name { get; }
    }
}
