using System.ComponentModel.DataAnnotations;

namespace Sumodh.Taskora.Api.Contracts.Auth.ResetPassword
{
    public sealed class RequestPasswordResetRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;
    }
}
