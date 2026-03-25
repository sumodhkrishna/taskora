namespace Sumodh.Taskora.Application.Abstractions.Authentication
{
    public interface IRefreshTokenGenerator
    {
        string Generate();
        string Hash(string token);
    }
}
