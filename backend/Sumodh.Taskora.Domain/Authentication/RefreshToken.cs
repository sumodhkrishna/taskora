namespace Sumodh.Taskora.Domain.Authentication
{
    public class RefreshToken
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }

        public string TokenHash { get; private set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }

        public bool IsRevoked { get; private set; }
        public DateTime? RevokedAtUtc { get; private set; }

        public string? ReplacedByTokenHash { get; private set; }

        private RefreshToken()
        {
        }

        public RefreshToken(int userId, string tokenHash, DateTime expiresAtUtc)
        {
            if (userId <= 0)
                throw new ArgumentException("User is required.", nameof(userId));

            if (string.IsNullOrWhiteSpace(tokenHash))
                throw new ArgumentException("Token hash is required.", nameof(tokenHash));

            UserId = userId;
            TokenHash = tokenHash;
            ExpiresAtUtc = expiresAtUtc;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public bool IsActive(DateTime utcNow)
        {
            return !IsRevoked && ExpiresAtUtc > utcNow;
        }

        public void Revoke(string? replacedByTokenHash = null)
        {
            if (IsRevoked)
                return;

            IsRevoked = true;
            RevokedAtUtc = DateTime.UtcNow;
            ReplacedByTokenHash = replacedByTokenHash;
        }
    }
}
