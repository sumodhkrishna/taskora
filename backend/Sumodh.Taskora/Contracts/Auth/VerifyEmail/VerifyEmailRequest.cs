using System.ComponentModel.DataAnnotations;

namespace Sumodh.Taskora.Api.Contracts.Auth.VerifyEmail
{
    public sealed class VerifyEmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        public string Token { get; init; } = string.Empty;
    }
}
