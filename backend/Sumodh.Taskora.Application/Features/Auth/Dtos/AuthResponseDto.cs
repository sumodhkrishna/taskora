using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Application.Features.Auth.Dtos
{
    public class AuthResponseDto
    {
        public string AccessToken { get; init; } = string.Empty;
        public int UserId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
    }
}
