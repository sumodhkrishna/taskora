using Sumodh.Taskora.Application.Features.Auth.Commands.RequestPasswordReset;
using Sumodh.Taskora.Application.Features.Auth.Commands.ResetPassword;
using Sumodh.Taskora.Domain.Users;
using Sumodh.Taskora.Test.TestDoubles;

namespace Sumodh.Taskora.Test.Application.Auth;

public class PasswordResetCommandHandlerTests
{
    [Fact]
    public async Task RequestHandle_WhenUserExists_GeneratesTokenPersistsHashAndReturnsRawToken()
    {
        var user = new User("Sumodh", "user@example.com", "stored-hash");
        var userRepository = new FakeUserRepository { UserByEmailResult = user };
        var tokenGenerator = new StubPasswordResetTokenGenerator
        {
            GenerateResult = "raw-reset-token",
            HashResult = "hashed-reset-token"
        };
        var emailSender = new StubPasswordResetEmailSender();
        var handler = new RequestPasswordResetCommandHandler(userRepository, tokenGenerator, emailSender);

        await handler.Handle(
            new RequestPasswordResetCommand("  USER@Example.com  "),
            CancellationToken.None);

        Assert.Equal("user@example.com", userRepository.LastGetByEmailArgument);
        Assert.Equal(1, tokenGenerator.GenerateCallCount);
        Assert.Equal("raw-reset-token", tokenGenerator.LastHashArgument);
        Assert.Equal("hashed-reset-token", user.PasswordResetTokenHash);
        Assert.NotNull(user.PasswordResetTokenExpiresAtUtc);
        Assert.Equal(1, userRepository.SaveChangesCallCount);
        Assert.Equal(1, emailSender.SendCallCount);
        Assert.Equal(("Sumodh", "user@example.com", "raw-reset-token"), emailSender.LastSendArguments);
    }

    [Fact]
    public async Task RequestHandle_WhenUserMissing_ReturnsNull()
    {
        var userRepository = new FakeUserRepository();
        var tokenGenerator = new StubPasswordResetTokenGenerator();
        var emailSender = new StubPasswordResetEmailSender();
        var handler = new RequestPasswordResetCommandHandler(userRepository, tokenGenerator, emailSender);

        await handler.Handle(
            new RequestPasswordResetCommand("user@example.com"),
            CancellationToken.None);

        Assert.Equal(0, tokenGenerator.GenerateCallCount);
        Assert.Equal(0, userRepository.SaveChangesCallCount);
        Assert.Equal(0, emailSender.SendCallCount);
    }

    [Fact]
    public async Task ResetHandle_WithValidUserAndToken_UpdatesPasswordAndClearsResetState()
    {
        var user = new User("Sumodh", "user@example.com", "old-hash");
        user.SetPasswordResetToken("hashed-reset-token", DateTime.UtcNow.AddMinutes(30));
        var userRepository = new FakeUserRepository { UserByEmailResult = user };
        var tokenGenerator = new StubPasswordResetTokenGenerator { HashResult = "hashed-reset-token" };
        var passwordHasher = new StubPasswordHasher { HashResult = "new-hash" };
        var handler = new ResetPasswordCommandHandler(userRepository, tokenGenerator, passwordHasher);

        var result = await handler.Handle(
            new ResetPasswordCommand("  USER@Example.com  ", "raw-token", "new-password"),
            CancellationToken.None);

        Assert.True(result);
        Assert.Equal("user@example.com", userRepository.LastGetByEmailArgument);
        Assert.Equal("raw-token", tokenGenerator.LastHashArgument);
        Assert.Equal("new-password", passwordHasher.LastHashArgument);
        Assert.Equal("new-hash", user.PasswordHash);
        Assert.Null(user.PasswordResetTokenHash);
        Assert.Equal(1, userRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ResetHandle_WhenUserMissing_ReturnsFalse()
    {
        var handler = new ResetPasswordCommandHandler(
            new FakeUserRepository(),
            new StubPasswordResetTokenGenerator(),
            new StubPasswordHasher());

        var result = await handler.Handle(
            new ResetPasswordCommand("user@example.com", "raw-token", "new-password"),
            CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task ResetHandle_WhenTokenInvalid_ReturnsFalse()
    {
        var user = new User("Sumodh", "user@example.com", "old-hash");
        user.SetPasswordResetToken("valid-token-hash", DateTime.UtcNow.AddMinutes(30));
        var userRepository = new FakeUserRepository { UserByEmailResult = user };
        var passwordHasher = new StubPasswordHasher();
        var tokenGenerator = new StubPasswordResetTokenGenerator { HashResult = "wrong-token-hash" };
        var handler = new ResetPasswordCommandHandler(
            userRepository,
            tokenGenerator,
            passwordHasher);

        var result = await handler.Handle(
            new ResetPasswordCommand("user@example.com", "wrong-token", "new-password"),
            CancellationToken.None);

        Assert.False(result);
        Assert.Equal("wrong-token", tokenGenerator.LastHashArgument);
        Assert.Null(passwordHasher.LastHashArgument);
        Assert.Equal(0, userRepository.SaveChangesCallCount);
    }
}
