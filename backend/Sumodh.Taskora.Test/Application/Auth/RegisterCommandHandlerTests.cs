using Sumodh.Taskora.Application.Features.Auth.Commands.Register;
using Sumodh.Taskora.Test.TestDoubles;

namespace Sumodh.Taskora.Test.Application.Auth;

public class RegisterCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithUniqueEmail_CreatesUserAndReturnsDto()
    {
        var userRepository = new FakeUserRepository();
        var passwordHasher = new StubPasswordHasher { HashResult = "hashed-value" };
        var handler = new RegisterCommandHandler(userRepository, passwordHasher);

        var result = await handler.Handle(
            new RegisterCommand("Sumodh", "  USER@Example.com  ", "secret123"),
            CancellationToken.None);

        Assert.Equal("user@example.com", userRepository.LastEmailExistsArgument);
        Assert.Equal("secret123", passwordHasher.LastHashArgument);
        Assert.NotNull(userRepository.AddedUser);
        Assert.Equal("Sumodh", userRepository.AddedUser!.Name);
        Assert.Equal("user@example.com", userRepository.AddedUser.Email);
        Assert.Equal("hashed-value", userRepository.AddedUser.PasswordHash);
        Assert.Equal(1, userRepository.SaveChangesCallCount);
        Assert.Equal("Sumodh", result.Name);
        Assert.Equal("user@example.com", result.Email);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ThrowsAndDoesNotPersist()
    {
        var userRepository = new FakeUserRepository { EmailExistsResult = true };
        var passwordHasher = new StubPasswordHasher();
        var handler = new RegisterCommandHandler(userRepository, passwordHasher);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new RegisterCommand("Sumodh", "user@example.com", "secret123"), CancellationToken.None));

        Assert.Equal("A user with this email already exists.", exception.Message);
        Assert.Null(userRepository.AddedUser);
        Assert.Equal(0, userRepository.SaveChangesCallCount);
        Assert.Null(passwordHasher.LastHashArgument);
    }
}
