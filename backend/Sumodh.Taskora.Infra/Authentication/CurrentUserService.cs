using Sumodh.Taskora.Application.Abstractions.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Sumodh.Taskora.Infra.Authentication
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int UserId
        {
            get
            {
                var value = _httpContextAccessor.HttpContext?
                    .User
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                if (!int.TryParse(value, out var userId))
                    throw new UnauthorizedAccessException("Authenticated user id was not found.");

                return userId;
            }
        }

        public string? Email =>
            _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

        public string? Name =>
            _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
    }
}
