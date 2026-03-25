using System.ComponentModel.DataAnnotations;

namespace Sumodh.Taskora.Api.Contracts.Auth.ResetPassword
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;
        [Required]
        public string Token { get; init; } = string.Empty;
        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string NewPassword { get; init; } = string.Empty;
    }
}
