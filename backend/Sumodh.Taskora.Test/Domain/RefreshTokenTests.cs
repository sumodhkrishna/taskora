using Sumodh.Taskora.Domain.Authentication;

namespace Sumodh.Taskora.Test.Domain;

public class RefreshTokenTests
{
    [Fact]
    public void Constructor_WithValidValues_SetsProperties()
    {
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var before = DateTime.UtcNow;

        var token = new RefreshToken(12, "token-hash", expiresAt);

        var after = DateTime.UtcNow;

        Assert.Equal(12, token.UserId);
        Assert.Equal("token-hash", token.TokenHash);
        Assert.Equal(expiresAt, token.ExpiresAtUtc);
        Assert.False(token.IsRevoked);
        Assert.Null(token.RevokedAtUtc);
        Assert.Null(token.ReplacedByTokenHash);
        Assert.InRange(token.CreatedAtUtc, before, after);
    }

    [Fact]
    public void Constructor_WithInvalidUserId_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => new RefreshToken(0, "token-hash", DateTime.UtcNow.AddDays(7)));

        Assert.Equal("userId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithBlankHash_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => new RefreshToken(1, " ", DateTime.UtcNow.AddDays(7)));

        Assert.Equal("tokenHash", exception.ParamName);
    }

    [Fact]
    public void IsActive_WhenNotExpiredAndNotRevoked_ReturnsTrue()
    {
        var token = new RefreshToken(1, "token-hash", DateTime.UtcNow.AddMinutes(5));

        var result = token.IsActive(DateTime.UtcNow);

        Assert.True(result);
    }

    [Fact]
    public void IsActive_WhenExpired_ReturnsFalse()
    {
        var token = new RefreshToken(1, "token-hash", DateTime.UtcNow.AddMinutes(-1));

        var result = token.IsActive(DateTime.UtcNow);

        Assert.False(result);
    }

    [Fact]
    public void Revoke_WithReplacementHash_MarksTokenRevoked()
    {
        var token = new RefreshToken(1, "token-hash", DateTime.UtcNow.AddDays(7));

        token.Revoke("replacement-hash");

        Assert.True(token.IsRevoked);
        Assert.NotNull(token.RevokedAtUtc);
        Assert.Equal("replacement-hash", token.ReplacedByTokenHash);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_DoesNotOverwriteValues()
    {
        var token = new RefreshToken(1, "token-hash", DateTime.UtcNow.AddDays(7));
        token.Revoke("replacement-hash");
        var revokedAt = token.RevokedAtUtc;

        token.Revoke("second-hash");

        Assert.Equal(revokedAt, token.RevokedAtUtc);
        Assert.Equal("replacement-hash", token.ReplacedByTokenHash);
    }
}
