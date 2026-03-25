using Sumodh.Taskora.Application.Abstractions.Authentication;
using System.Security.Cryptography;
using System.Text;

namespace Sumodh.Taskora.Infra.Authentication
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool Verify(string password, string passwordHash)
        {
            var hashedInput = Hash(password);
            return hashedInput == passwordHash;
        }
    }
}
