using Microsoft.AspNetCore.Mvc;
using Sumodh.Taskora.Api.Controllers;
using Sumodh.Taskora.Application.Features.Users.Queries.GetById;
using Sumodh.Taskora.Test.TestDoubles;

namespace Sumodh.Taskora.Test.Controllers;

public class UsersControllerTests
{
    [Fact]
    public async Task GetUserById_WhenRequestedIdDoesNotMatchCurrentUser_ReturnsForbid()
    {
        var controller = new UsersController();
        var handler = new GetUserByIdQueryHandler(new FakeUserRepository());

        var result = await controller.GetUserById(
            9,
            handler,
            new FakeCurrentUserService { UserId = 5 },
            CancellationToken.None);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task GetUserById_WhenRequestedIdMatchesCurrentUser_ReturnsOk()
    {
        var controller = new UsersController();
        var handler = new GetUserByIdQueryHandler(new FakeUserRepository
        {
            UserByIdResult = new Sumodh.Taskora.Domain.Users.User("Sumodh", "user@example.com", "hash") { Id = 5 }
        });

        var result = await controller.GetUserById(
            5,
            handler,
            new FakeCurrentUserService { UserId = 5 },
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var user = Assert.IsType<Sumodh.Taskora.Api.Contracts.Users.UserDto>(okResult.Value);
        Assert.Equal(5, user.Id);
        Assert.Equal("user@example.com", user.Email);
    }
}
