using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Application.Abstractions.Authentication
{
    public interface IJWTTokenGenerator
    {
        string GenerateToken(int userId, string email, string name);
    }
}
