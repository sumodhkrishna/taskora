using Sumodh.Taskora.Application.Features.Users.Queries.GetById;
using Sumodh.Taskora.Application.Features.Users.Queries.GetCurrentUser;
using Sumodh.Taskora.Domain.Users;
using Sumodh.Taskora.Test.TestDoubles;

namespace Sumodh.Taskora.Test.Application.Users;

public class UserQueryHandlerTests
{
    [Fact]
    public async Task GetUserByIdHandle_WhenUserExists_ReturnsMappedDto()
    {
        var userRepository = new FakeUserRepository
        {
            UserByIdResult = new User("Sumodh", "user@example.com", "hash") { Id = 10 }
        };
        var handler = new GetUserByIdQueryHandler(userRepository);

        var result = await handler.Handle(new GetUserByIdQuery(10), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(10, result!.Id);
        Assert.Equal("Sumodh", result.Name);
        Assert.Equal("user@example.com", result.Email);
    }

    [Fact]
    public async Task GetUserByIdHandle_WhenUserMissing_ReturnsNull()
    {
        var handler = new GetUserByIdQueryHandler(new FakeUserRepository());

        var result = await handler.Handle(new GetUserByIdQuery(10), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentUserHandle_WhenUserExists_ReturnsMappedDto()
    {
        var userRepository = new FakeUserRepository
        {
            UserByIdResult = new User("Sumodh", "user@example.com", "hash") { Id = 15 }
        };
        var handler = new GetCurrentUserQueryHandler(
            userRepository,
            new FakeCurrentUserService { UserId = 15 });

        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(15, result!.Id);
        Assert.Equal("Sumodh", result.Name);
        Assert.Equal("user@example.com", result.Email);
    }

    [Fact]
    public async Task GetCurrentUserHandle_WhenUserMissing_ReturnsNull()
    {
        var handler = new GetCurrentUserQueryHandler(
            new FakeUserRepository(),
            new FakeCurrentUserService { UserId = 15 });

        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        Assert.Null(result);
    }
}
