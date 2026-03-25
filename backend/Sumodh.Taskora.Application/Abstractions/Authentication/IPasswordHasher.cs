using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Application.Abstractions.Authentication
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string passwordHash);
    }
}
