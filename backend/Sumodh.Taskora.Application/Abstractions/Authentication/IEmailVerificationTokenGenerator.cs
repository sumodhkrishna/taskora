namespace Sumodh.Taskora.Application.Abstractions.Authentication
{
    public interface IEmailVerificationTokenGenerator
    {
        string Generate();
        string Hash(string token);
    }
}
