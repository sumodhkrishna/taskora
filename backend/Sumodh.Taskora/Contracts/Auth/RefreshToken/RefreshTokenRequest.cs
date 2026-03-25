using System.ComponentModel.DataAnnotations;

namespace Sumodh.Taskora.Api.Contracts.Auth.RefreshToken
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; init; } = string.Empty;
    }
}
