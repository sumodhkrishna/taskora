using Sumodh.Taskora.Domain.Users;

namespace Sumodh.Taskora.Test.Domain;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidValues_SetsProperties()
    {
        var before = DateTime.UtcNow;

        var user = new User("Sumodh", "  USER@Example.com  ", "hash");

        var after = DateTime.UtcNow;

        Assert.Equal("Sumodh", user.Name);
        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("hash", user.PasswordHash);
        Assert.InRange(user.CreatedAt, before, after);
        Assert.Empty(user.TodoItems);
        Assert.Empty(user.RefreshTokens);
        Assert.Null(user.EmailVerifiedAtUtc);
        Assert.Null(user.EmailVerificationTokenHash);
        Assert.Null(user.EmailVerificationTokenExpiresAtUtc);
        Assert.Null(user.PasswordResetTokenHash);
        Assert.Null(user.PasswordResetTokenExpiresAtUtc);
        Assert.Null(user.PasswordResetRequestedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithInvalidName_Throws(string name)
    {
        Assert.Throws<InvalidDataException>(() => new User(name, "user@example.com", "hash"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithInvalidEmail_Throws(string email)
    {
        Assert.Throws<InvalidDataException>(() => new User("Sumodh", email, "hash"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithInvalidPasswordHash_Throws(string passwordHash)
    {
        Assert.Throws<InvalidDataException>(() => new User("Sumodh", "user@example.com", passwordHash));
    }

    [Fact]
    public void SetPasswordResetToken_WithValidValues_SetsResetFields()
    {
        var user = new User("Sumodh", "user@example.com", "hash");
        var expiresAt = DateTime.UtcNow.AddMinutes(30);
        var before = DateTime.UtcNow;

        user.SetPasswordResetToken("reset-hash", expiresAt);

        var after = DateTime.UtcNow;

        Assert.Equal("reset-hash", user.PasswordResetTokenHash);
        Assert.Equal(expiresAt, user.PasswordResetTokenExpiresAtUtc);
        Assert.NotNull(user.PasswordResetRequestedAtUtc);
        Assert.InRange(user.PasswordResetRequestedAtUtc!.Value, before, after);
    }

    [Fact]
    public void SetEmailVerificationToken_WithValidValues_SetsVerificationFields()
    {
        var user = new User("Sumodh", "user@example.com", "hash");
        var expiresAt = DateTime.UtcNow.AddHours(24);

        user.SetEmailVerificationToken("verification-hash", expiresAt);

        Assert.Equal("verification-hash", user.EmailVerificationTokenHash);
        Assert.Equal(expiresAt, user.EmailVerificationTokenExpiresAtUtc);
        Assert.False(user.IsEmailVerified);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void SetEmailVerificationToken_WithInvalidHash_Throws(string tokenHash)
    {
        var user = new User("Sumodh", "user@example.com", "hash");

        var exception = Assert.Throws<ArgumentException>(() => user.SetEmailVerificationToken(tokenHash, DateTime.UtcNow.AddHours(24)));

        Assert.Equal("tokenHash", exception.ParamName);
    }

    [Fact]
    public void CanUseEmailVerificationToken_WhenAllConditionsMatch_ReturnsTrue()
    {
        var user = new User("Sumodh", "user@example.com", "hash");
        var now = DateTime.UtcNow;
        user.SetEmailVerificationToken("verification-hash", now.AddHours(24));

        var result = user.CanUseEmailVerificationToken("verification-hash", now);

        Assert.True(result);
    }

    [Fact]
    public void CanUseEmailVerificationToken_WhenExpired_ReturnsFalse()
    {
        var user = new User("Sumodh", "user@example.com", "hash");
        var now = DateTime.UtcNow;
        user.SetEmailVerificationToken("verification-hash", now.AddMinutes(10));

        var result = user.CanUseEmailVerificationToken("verification-hash", now.AddMinutes(11));

        Assert.False(result);
    }

    [Fact]
    public void MarkEmailVerified_SetsVerifiedAtAndClearsVerificationToken()
    {
        var user = new User("Sumodh", "user@example.com", "hash");
        var verifiedAt = DateTime.UtcNow;
        user.SetEmailVerificationToken("verification-hash", verifiedAt.AddHours(24));

        user.MarkEmailVerified(verifiedAt);

        Assert.True(user.IsEmailVerified);
        Assert.Equal(verifiedAt, user.EmailVerifiedAtUtc);
        Assert.Null(user.EmailVerificationTokenHash);
        Assert.Null(user.EmailVerificationTokenExpiresAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void SetPasswordResetToken_WithInvalidHash_Throws(string tokenHash)
    {
        var user = new User("Sumodh", "user@example.com", "hash");

        var exception = Assert.Throws<ArgumentException>(() => user.SetPasswordResetToken(tokenHash, DateTime.UtcNow.AddMinutes(30)));

        Assert.Equal("tokenHash", exception.ParamName);
    }

    [Fact]
    public void CanUsePasswordResetToken_WhenAllConditionsMatch_ReturnsTrue()
    {
        var user = new User("Sumodh", "user@example.com", "hash");
        var now = DateTime.UtcNow;
        user.SetPasswordResetToken("reset-hash", now.AddMinutes(30));

        var result = user.CanUsePasswordResetToken("reset-hash", now);

        Assert.True(result);
    }

    [Fact]
    public void CanUsePasswordResetToken_WhenTokenDiffers_ReturnsFalse()
    {
        var user = new User("Sumodh", "user@example.com", "hash");
        user.SetPasswordResetToken("reset-hash", DateTime.UtcNow.AddMinutes(30));

        var result = user.CanUsePasswordResetToken("other-hash", DateTime.UtcNow);

        Assert.False(result);
    }

    [Fact]
    public void CanUsePasswordResetToken_WhenExpired_ReturnsFalse()
    {
        var user = new User("Sumodh", "user@example.com", "hash");
        var now = DateTime.UtcNow;
        user.SetPasswordResetToken("reset-hash", now.AddMinutes(1));

        var result = user.CanUsePasswordResetToken("reset-hash", now.AddMinutes(2));

        Assert.False(result);
    }

    [Fact]
    public void ResetPassword_WithValidHash_UpdatesPasswordAndClearsResetState()
    {
        var user = new User("Sumodh", "user@example.com", "old-hash");
        user.SetPasswordResetToken("reset-hash", DateTime.UtcNow.AddMinutes(30));

        user.ResetPassword("new-hash");

        Assert.Equal("new-hash", user.PasswordHash);
        Assert.Null(user.PasswordResetTokenHash);
        Assert.Null(user.PasswordResetTokenExpiresAtUtc);
        Assert.Null(user.PasswordResetRequestedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ResetPassword_WithInvalidHash_Throws(string newPasswordHash)
    {
        var user = new User("Sumodh", "user@example.com", "old-hash");

        var exception = Assert.Throws<ArgumentException>(() => user.ResetPassword(newPasswordHash));

        Assert.Equal("newPasswordHash", exception.ParamName);
    }

    [Fact]
    public void ClearPasswordResetToken_ClearsResetState()
    {
        var user = new User("Sumodh", "user@example.com", "hash");
        user.SetPasswordResetToken("reset-hash", DateTime.UtcNow.AddMinutes(30));

        user.ClearPasswordResetToken();

        Assert.Null(user.PasswordResetTokenHash);
        Assert.Null(user.PasswordResetTokenExpiresAtUtc);
        Assert.Null(user.PasswordResetRequestedAtUtc);
    }
}
