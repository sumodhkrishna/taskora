using Sumodh.Taskora.Application.Features.Auth.Commands.ResendEmailVerification;
using Sumodh.Taskora.Application.Features.Auth.Commands.VerifyEmail;
using Sumodh.Taskora.Domain.Users;
using Sumodh.Taskora.Test.TestDoubles;

namespace Sumodh.Taskora.Test.Application.Auth;

public class EmailVerificationCommandHandlerTests
{
    [Fact]
    public async Task VerifyHandle_WithValidUserAndToken_MarksUserVerified()
    {
        var user = new User("Sumodh", "user@example.com", "hash");
        user.SetEmailVerificationToken("hashed-token", DateTime.UtcNow.AddHours(2));

        var userRepository = new FakeUserRepository { UserByEmailResult = user };
        var tokenGenerator = new StubEmailVerificationTokenGenerator { HashResult = "hashed-token" };
        var handler = new VerifyEmailCommandHandler(userRepository, tokenGenerator);

        var result = await handler.Handle(
            new VerifyEmailCommand(" USER@example.com ", "raw-token"),
            CancellationToken.None);

        Assert.True(result);
        Assert.Equal("raw-token", tokenGenerator.LastHashArgument);
        Assert.True(user.IsEmailVerified);
        Assert.Null(user.EmailVerificationTokenHash);
        Assert.Equal(1, userRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task VerifyHandle_WhenTokenInvalid_ReturnsFalse()
    {
        var user = new User("Sumodh", "user@example.com", "hash");
        user.SetEmailVerificationToken("expected-hash", DateTime.UtcNow.AddHours(2));

        var userRepository = new FakeUserRepository { UserByEmailResult = user };
        var tokenGenerator = new StubEmailVerificationTokenGenerator { HashResult = "wrong-hash" };
        var handler = new VerifyEmailCommandHandler(userRepository, tokenGenerator);

        var result = await handler.Handle(
            new VerifyEmailCommand("user@example.com", "wrong-token"),
            CancellationToken.None);

        Assert.False(result);
        Assert.False(user.IsEmailVerified);
        Assert.Equal(0, userRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ResendHandle_WithUnverifiedUser_GeneratesTokenPersistsHashAndSendsEmail()
    {
        var user = new User("Sumodh", "user@example.com", "hash");
        var userRepository = new FakeUserRepository { UserByEmailResult = user };
        var tokenGenerator = new StubEmailVerificationTokenGenerator
        {
            GenerateResult = "new-raw-token",
            HashResult = "new-hash"
        };
        var emailSender = new StubEmailVerificationEmailSender();
        var handler = new ResendEmailVerificationCommandHandler(userRepository, tokenGenerator, emailSender);

        var result = await handler.Handle(new ResendEmailVerificationCommand(" user@example.com "), CancellationToken.None);

        Assert.True(result);
        Assert.Equal("user@example.com", userRepository.LastGetByEmailArgument);
        Assert.Equal(1, tokenGenerator.GenerateCallCount);
        Assert.Equal("new-raw-token", tokenGenerator.LastHashArgument);
        Assert.Equal("new-hash", user.EmailVerificationTokenHash);
        Assert.Equal(1, userRepository.SaveChangesCallCount);
        Assert.Equal(1, emailSender.SendCallCount);
        Assert.Equal(("Sumodh", "user@example.com", "new-raw-token"), emailSender.LastSendArguments);
    }

    [Fact]
    public async Task ResendHandle_WhenUserAlreadyVerified_DoesNothing()
    {
        var user = new User("Sumodh", "user@example.com", "hash");
        user.MarkEmailVerified(DateTime.UtcNow);

        var userRepository = new FakeUserRepository { UserByEmailResult = user };
        var tokenGenerator = new StubEmailVerificationTokenGenerator();
        var emailSender = new StubEmailVerificationEmailSender();
        var handler = new ResendEmailVerificationCommandHandler(userRepository, tokenGenerator, emailSender);

        var result = await handler.Handle(new ResendEmailVerificationCommand("user@example.com"), CancellationToken.None);

        Assert.False(result);
        Assert.Equal(0, tokenGenerator.GenerateCallCount);
        Assert.Equal(0, userRepository.SaveChangesCallCount);
        Assert.Equal(0, emailSender.SendCallCount);
    }
}
