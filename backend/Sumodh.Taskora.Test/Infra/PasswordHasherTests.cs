using Sumodh.Taskora.Infra.Authentication;

namespace Sumodh.Taskora.Test.Infra;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_SamePassword_ReturnsSameHash()
    {
        var hasher = new PasswordHasher();

        var first = hasher.Hash("secret123");
        var second = hasher.Hash("secret123");

        Assert.Equal(first, second);
    }

    [Fact]
    public void Verify_WithMatchingPassword_ReturnsTrue()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("secret123");

        var result = hasher.Verify("secret123", hash);

        Assert.True(result);
    }

    [Fact]
    public void Verify_WithDifferentPassword_ReturnsFalse()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("secret123");

        var result = hasher.Verify("different", hash);

        Assert.False(result);
    }
}
