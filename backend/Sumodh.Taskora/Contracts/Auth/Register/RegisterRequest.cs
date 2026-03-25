using System.ComponentModel.DataAnnotations;

namespace Sumodh.Taskora.Api.Contracts.Auth.Register
{
    public class RegisterRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; init; } = string.Empty;
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; init; } = string.Empty;
        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string Password { get; init; } = string.Empty;
    }
}
