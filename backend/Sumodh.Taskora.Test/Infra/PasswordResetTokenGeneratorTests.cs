using Sumodh.Taskora.Infra.Authentication;

namespace Sumodh.Taskora.Test.Infra;

public class PasswordResetTokenGeneratorTests
{
    [Fact]
    public void Generate_ReturnsNonEmptyToken()
    {
        var generator = new PasswordResetTokenGenerator();

        var token = generator.Generate();

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void Generate_Twice_ReturnsDifferentTokens()
    {
        var generator = new PasswordResetTokenGenerator();

        var first = generator.Generate();
        var second = generator.Generate();

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Hash_SameToken_ReturnsSameHash()
    {
        var generator = new PasswordResetTokenGenerator();

        var first = generator.Hash("reset-token");
        var second = generator.Hash("reset-token");

        Assert.Equal(first, second);
    }

    [Fact]
    public void Hash_DifferentTokens_ReturnsDifferentHashes()
    {
        var generator = new PasswordResetTokenGenerator();

        var first = generator.Hash("reset-token-one");
        var second = generator.Hash("reset-token-two");

        Assert.NotEqual(first, second);
    }
}
