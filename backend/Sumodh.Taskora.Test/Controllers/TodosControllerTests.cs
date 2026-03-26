using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sumodh.Taskora.Api.Contracts.Todos;
using Sumodh.Taskora.Api.Controllers;
using Sumodh.Taskora.Application.Features.Todos.Commands;
using Sumodh.Taskora.Application.Features.Todos.Dtos;
using Sumodh.Taskora.Application.Features.Todos.Queries;
using Sumodh.Taskora.Domain.Todos;
using Sumodh.Taskora.Test.TestDoubles;

namespace Sumodh.Taskora.Test.Controllers;

public class TodosControllerTests
{
    [Fact]
    public async Task Create_ReturnsCreatedTodo()
    {
        var controller = new TodosController();
        var handler = new CreateTodoCommandHandler(
            new FakeTodoRepository(),
            new FakeCurrentUserService { UserId = 5 });

        var result = await controller.Create(
            new CreateTodoRequest
            {
                Title = "Finish docs",
                Description = "Ship tests",
                Priority = (int)TodoPriority.High
            },
            handler,
            CancellationToken.None);

        var created = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        var dto = Assert.IsType<TodoDto>(created.Value);
        Assert.Equal("Finish docs", dto.Title);
        Assert.Equal((int)TodoPriority.High, dto.Priority);
    }

    [Fact]
    public async Task GetMine_MapsCompletedStatusAndReturnsOk()
    {
        var todoRepository = new FakeTodoRepository
        {
            PagedTodosResult = [new TodoItem(5, "Done", TodoPriority.High, null, null)],
            PagedTotalCountResult = 1
        };
        todoRepository.PagedTodosResult[0].MarkComplete();

        var controller = new TodosController();
        var handler = new GetMyTodosQueryHandler(todoRepository, new FakeCurrentUserService { UserId = 5 });

        var result = await controller.GetMine(
            new GetMyTodosRequest
            {
                Status = " completed ",
                Page = 1,
                PageSize = 20
            },
            handler,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<PagedResultDto<TodoDto>>(okResult.Value);
        Assert.Equal((5, true, null, null, null, 1, 20), todoRepository.LastGetPagedForUserArguments);
        Assert.Single(dto.Items);
        Assert.True(dto.Items[0].IsCompleted);
    }

    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        var controller = new TodosController();
        var handler = new GetTodoByIdQueryHandler(
            new FakeTodoRepository(),
            new FakeCurrentUserService { UserId = 5 });

        var result = await controller.GetById(9, handler, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_WhenMissing_ReturnsNotFound()
    {
        var controller = new TodosController();
        var handler = new DeleteTodoCommandHandler(
            new FakeTodoRepository(),
            new FakeCurrentUserService { UserId = 5 });

        var result = await controller.Delete(3, handler, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}
