using Sumodh.Taskora.Application.Features.Auth.Commands.Register;
using Sumodh.Taskora.Test.TestDoubles;

namespace Sumodh.Taskora.Test.Application.Auth;

public class RegisterCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithUniqueEmail_CreatesUnverifiedUserAndSendsVerification()
    {
        var userRepository = new FakeUserRepository();
        var passwordHasher = new StubPasswordHasher { HashResult = "hashed-value" };
        var verificationTokenGenerator = new StubEmailVerificationTokenGenerator
        {
            GenerateResult = "raw-verify-token",
            HashResult = "hashed-verify-token"
        };
        var verificationEmailSender = new StubEmailVerificationEmailSender();
        var handler = new RegisterCommandHandler(
            userRepository,
            passwordHasher,
            verificationTokenGenerator,
            verificationEmailSender);

        var result = await handler.Handle(
            new RegisterCommand("Sumodh", "  USER@Example.com  ", "secret123"),
            CancellationToken.None);

        Assert.Equal("user@example.com", userRepository.LastEmailExistsArgument);
        Assert.Equal("secret123", passwordHasher.LastHashArgument);
        Assert.Equal(1, verificationTokenGenerator.GenerateCallCount);
        Assert.Equal("raw-verify-token", verificationTokenGenerator.LastHashArgument);
        Assert.NotNull(userRepository.AddedUser);
        Assert.Equal("Sumodh", userRepository.AddedUser!.Name);
        Assert.Equal("user@example.com", userRepository.AddedUser.Email);
        Assert.Equal("hashed-value", userRepository.AddedUser.PasswordHash);
        Assert.False(userRepository.AddedUser.IsEmailVerified);
        Assert.Equal("hashed-verify-token", userRepository.AddedUser.EmailVerificationTokenHash);
        Assert.Equal(1, userRepository.SaveChangesCallCount);
        Assert.Equal(1, verificationEmailSender.SendCallCount);
        Assert.Equal(("Sumodh", "user@example.com", "raw-verify-token"), verificationEmailSender.LastSendArguments);
        Assert.Equal("Sumodh", result.Name);
        Assert.Equal("user@example.com", result.Email);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ThrowsAndDoesNotPersist()
    {
        var userRepository = new FakeUserRepository { EmailExistsResult = true };
        var passwordHasher = new StubPasswordHasher();
        var verificationTokenGenerator = new StubEmailVerificationTokenGenerator();
        var verificationEmailSender = new StubEmailVerificationEmailSender();
        var handler = new RegisterCommandHandler(
            userRepository,
            passwordHasher,
            verificationTokenGenerator,
            verificationEmailSender);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new RegisterCommand("Sumodh", "user@example.com", "secret123"), CancellationToken.None));

        Assert.Equal("A user with this email already exists.", exception.Message);
        Assert.Null(userRepository.AddedUser);
        Assert.Equal(0, userRepository.SaveChangesCallCount);
        Assert.Null(passwordHasher.LastHashArgument);
        Assert.Equal(0, verificationTokenGenerator.GenerateCallCount);
        Assert.Equal(0, verificationEmailSender.SendCallCount);
    }
}
