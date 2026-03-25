using Sumodh.Taskora.Application.Features.Auth.Commands.RefreshToken;
using Sumodh.Taskora.Domain.Authentication;
using Sumodh.Taskora.Domain.Users;
using Sumodh.Taskora.Test.TestDoubles;

namespace Sumodh.Taskora.Test.Application.Auth;

public class RefreshTokenCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithActiveTokenAndUser_RotatesTokenAndReturnsAuthResponse()
    {
        var existingToken = new RefreshToken(17, "hashed-token", DateTime.UtcNow.AddDays(7));
        var refreshTokenRepository = new FakeRefreshTokenRepository
        {
            RefreshTokenByHashResult = existingToken
        };
        var userRepository = new FakeUserRepository
        {
            UserByIdResult = new User("Sumodh", "user@example.com", "stored-hash") { Id = 17 }
        };
        var jwtTokenGenerator = new StubJwtTokenGenerator { TokenToReturn = "new-access-token" };
        var refreshTokenGenerator = new StubRefreshTokenGenerator
        {
            GenerateResult = "new-refresh-token",
            HashResult = "hashed-token"
        };
        var handler = new RefreshTokenCommandHandler(
            refreshTokenRepository,
            userRepository,
            jwtTokenGenerator,
            refreshTokenGenerator);

        var result = await handler.Handle(new RefreshTokenCommand("incoming-raw-token"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(["incoming-raw-token", "new-refresh-token"], refreshTokenGenerator.HashArguments);
        Assert.Equal("hashed-token", refreshTokenRepository.LastGetByTokenHashArgument);
        Assert.True(existingToken.IsRevoked);
        Assert.Equal("hashed-token", existingToken.ReplacedByTokenHash);
        Assert.NotNull(refreshTokenRepository.AddedRefreshToken);
        Assert.Equal(17, refreshTokenRepository.AddedRefreshToken!.UserId);
        Assert.Equal("hashed-token", refreshTokenRepository.AddedRefreshToken.TokenHash);
        Assert.Equal(1, refreshTokenRepository.SaveChangesCallCount);
        Assert.Equal((17, "user@example.com", "Sumodh"), jwtTokenGenerator.LastGenerateArguments);
        Assert.Equal("new-access-token", result!.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
    }

    [Fact]
    public async Task Handle_WhenExistingTokenMissing_ReturnsNull()
    {
        var refreshTokenGenerator = new StubRefreshTokenGenerator { HashResult = "hashed-token" };
        var handler = new RefreshTokenCommandHandler(
            new FakeRefreshTokenRepository(),
            new FakeUserRepository(),
            new StubJwtTokenGenerator(),
            refreshTokenGenerator);

        var result = await handler.Handle(new RefreshTokenCommand("incoming-raw-token"), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WhenExistingTokenInactive_ReturnsNull()
    {
        var expiredToken = new RefreshToken(17, "hashed-token", DateTime.UtcNow.AddMinutes(-1));
        var handler = new RefreshTokenCommandHandler(
            new FakeRefreshTokenRepository { RefreshTokenByHashResult = expiredToken },
            new FakeUserRepository(),
            new StubJwtTokenGenerator(),
            new StubRefreshTokenGenerator { HashResult = "hashed-token" });

        var result = await handler.Handle(new RefreshTokenCommand("incoming-raw-token"), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WhenUserMissing_ReturnsNull()
    {
        var activeToken = new RefreshToken(17, "hashed-token", DateTime.UtcNow.AddDays(7));
        var handler = new RefreshTokenCommandHandler(
            new FakeRefreshTokenRepository { RefreshTokenByHashResult = activeToken },
            new FakeUserRepository(),
            new StubJwtTokenGenerator(),
            new StubRefreshTokenGenerator { HashResult = "hashed-token" });

        var result = await handler.Handle(new RefreshTokenCommand("incoming-raw-token"), CancellationToken.None);

        Assert.Null(result);
    }
}
