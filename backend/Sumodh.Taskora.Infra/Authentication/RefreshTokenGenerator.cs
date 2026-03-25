using Sumodh.Taskora.Application.Abstractions.Authentication;
using System.Security.Cryptography;
using System.Text;

namespace Sumodh.Taskora.Infra.Authentication
{
    public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        public string Generate()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes);
        }

        public string Hash(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
