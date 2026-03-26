namespace Sumodh.Taskora.Application.Features.Auth.Exceptions
{
    public sealed class EmailNotVerifiedException : Exception
    {
        public EmailNotVerifiedException()
            : base("Please verify your email address before signing in.")
        {
        }
    }
}
