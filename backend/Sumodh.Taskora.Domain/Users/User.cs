using Sumodh.Taskora.Domain.Authentication;
using Sumodh.Taskora.Domain.Todos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Sumodh.Taskora.Domain.Users
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? PasswordResetTokenHash { get; private set; }
        public DateTime? PasswordResetTokenExpiresAtUtc { get; private set; }
        public DateTime? PasswordResetRequestedAtUtc { get; private set; }

        public ICollection<TodoItem> TodoItems { get; private set; } = new List<TodoItem>();
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
        public User(string name, string email, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidDataException("Name is required.");

            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidDataException("Email is required.");

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new InvalidDataException("Password hash is required.");
            Email = email.Trim().ToLowerInvariant();
            Name = name;
            PasswordHash = passwordHash;
            CreatedAt = DateTime.UtcNow;
        }

        public void SetPasswordResetToken(string tokenHash, DateTime expiresAtUtc)
        {
            if (string.IsNullOrWhiteSpace(tokenHash))
                throw new ArgumentException("Token hash is required.", nameof(tokenHash));

            PasswordResetTokenHash = tokenHash;
            PasswordResetTokenExpiresAtUtc = expiresAtUtc;
            PasswordResetRequestedAtUtc = DateTime.UtcNow;
        }

        public bool CanUsePasswordResetToken(string tokenHash, DateTime utcNow)
        {
            if (string.IsNullOrWhiteSpace(tokenHash))
                return false;

            if (string.IsNullOrWhiteSpace(PasswordResetTokenHash))
                return false;

            if (PasswordResetTokenExpiresAtUtc is null || PasswordResetTokenExpiresAtUtc <= utcNow)
                return false;

            return PasswordResetTokenHash == tokenHash;
        }

        public void ResetPassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Password hash is required.", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
            ClearPasswordResetToken();
        }

        public void ClearPasswordResetToken()
        {
            PasswordResetTokenHash = null;
            PasswordResetTokenExpiresAtUtc = null;
            PasswordResetRequestedAtUtc = null;
        }
    }

}
