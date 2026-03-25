using System.ComponentModel.DataAnnotations;

namespace Sumodh.Taskora.Api.Contracts.Auth.Logout
{
    public class LogoutRequest
    {
        [Required]
        public string RefreshToken { get; init; } = string.Empty;
    }
}
