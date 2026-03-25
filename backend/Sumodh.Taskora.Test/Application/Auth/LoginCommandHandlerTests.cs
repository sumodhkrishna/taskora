using Sumodh.Taskora.Application.Features.Auth.Commands.Login;
using Sumodh.Taskora.Domain.Users;
using Sumodh.Taskora.Test.TestDoubles;

namespace Sumodh.Taskora.Test.Application.Auth;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsAuthResponse()
    {
        var userRepository = new FakeUserRepository
        {
            UserByEmailResult = new User("Sumodh", "user@example.com", "stored-hash") { Id = 17 }
        };
        var passwordHasher = new StubPasswordHasher { VerifyResult = true };
        var jwtTokenGenerator = new StubJwtTokenGenerator { TokenToReturn = "access-token" };
        var refreshTokenGenerator = new StubRefreshTokenGenerator
        {
            GenerateResult = "refresh-token",
            HashResult = "hashed-refresh-token"
        };
        var refreshTokenRepository = new FakeRefreshTokenRepository();
        var handler = new LoginCommandHandler(
            userRepository,
            passwordHasher,
            jwtTokenGenerator,
            refreshTokenGenerator,
            refreshTokenRepository);

        var result = await handler.Handle(
            new LoginCommand("  USER@Example.com  ", "secret123"),
            CancellationToken.None);

        Assert.Equal("user@example.com", userRepository.LastGetByEmailArgument);
        Assert.Equal(("secret123", "stored-hash"), passwordHasher.LastVerifyArguments);
        Assert.Equal((17, "user@example.com", "Sumodh"), jwtTokenGenerator.LastGenerateArguments);
        Assert.Equal(1, refreshTokenGenerator.GenerateCallCount);
        Assert.Equal("refresh-token", refreshTokenGenerator.LastHashArgument);
        Assert.NotNull(refreshTokenRepository.AddedRefreshToken);
        Assert.Equal(17, refreshTokenRepository.AddedRefreshToken!.UserId);
        Assert.Equal("hashed-refresh-token", refreshTokenRepository.AddedRefreshToken.TokenHash);
        Assert.Equal(1, refreshTokenRepository.SaveChangesCallCount);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal(17, result.UserId);
        Assert.Equal("Sumodh", result.Name);
        Assert.Equal("user@example.com", result.Email);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsUnauthorized()
    {
        var handler = new LoginCommandHandler(
            new FakeUserRepository(),
            new StubPasswordHasher(),
            new StubJwtTokenGenerator(),
            new StubRefreshTokenGenerator(),
            new FakeRefreshTokenRepository());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new LoginCommand("user@example.com", "secret123"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_ThrowsUnauthorized()
    {
        var userRepository = new FakeUserRepository
        {
            UserByEmailResult = new User("Sumodh", "user@example.com", "stored-hash")
        };
        var handler = new LoginCommandHandler(
            userRepository,
            new StubPasswordHasher { VerifyResult = false },
            new StubJwtTokenGenerator(),
            new StubRefreshTokenGenerator(),
            new FakeRefreshTokenRepository());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new LoginCommand("user@example.com", "wrong-password"), CancellationToken.None));
    }
}
