using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Application.Features.Auth.Commands.RequestPasswordReset
{
    public sealed record RequestPasswordResetCommand(string Email);
}
