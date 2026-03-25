using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Application.Features.Auth.Commands.ResetPassword
{
    public sealed record ResetPasswordCommand(string Email,string Token,string NewPassword);
}
