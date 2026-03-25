using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Application.Abstractions.Authentication
{
    public interface IPasswordResetTokenGenerator
    {
        string Generate();
        string Hash(string token);
    }
}
