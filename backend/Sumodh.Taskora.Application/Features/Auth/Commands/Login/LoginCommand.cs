using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Application.Features.Auth.Commands.Login
{
    public sealed record LoginCommand(string Email, string Password);
}
