using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Application.Features.Auth.Commands.Logout
{
    public sealed record LogoutCommand(string RefreshToken);
}
