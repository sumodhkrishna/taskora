using Sumodh.Taskora.Application.Features.Auth.Commands.Logout;
using Sumodh.Taskora.Domain.Authentication;
using Sumodh.Taskora.Test.TestDoubles;

namespace Sumodh.Taskora.Test.Application.Auth;

public class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithMatchingToken_RevokesAndSaves()
    {
        var existingToken = new RefreshToken(17, "token-hash", DateTime.UtcNow.AddDays(7));
        var refreshTokenRepository = new FakeRefreshTokenRepository
        {
            RefreshTokenByHashResult = existingToken
        };
        var refreshTokenGenerator = new StubRefreshTokenGenerator { HashResult = "token-hash" };
        var handler = new LogoutCommandHandler(refreshTokenRepository, refreshTokenGenerator);

        await handler.Handle(new LogoutCommand("raw-token"), CancellationToken.None);

        Assert.Equal("raw-token", refreshTokenGenerator.LastHashArgument);
        Assert.Equal("token-hash", refreshTokenRepository.LastGetByTokenHashArgument);
        Assert.True(existingToken.IsRevoked);
        Assert.Equal(1, refreshTokenRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenTokenIsMissing_DoesNothing()
    {
        var refreshTokenRepository = new FakeRefreshTokenRepository();
        var refreshTokenGenerator = new StubRefreshTokenGenerator { HashResult = "token-hash" };
        var handler = new LogoutCommandHandler(refreshTokenRepository, refreshTokenGenerator);

        await handler.Handle(new LogoutCommand("raw-token"), CancellationToken.None);

        Assert.Equal("token-hash", refreshTokenRepository.LastGetByTokenHashArgument);
        Assert.Equal(0, refreshTokenRepository.SaveChangesCallCount);
    }
}
