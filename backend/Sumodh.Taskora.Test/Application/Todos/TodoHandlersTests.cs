using Sumodh.Taskora.Application.Features.Todos.Commands;
using Sumodh.Taskora.Application.Features.Todos.Queries;
using Sumodh.Taskora.Domain.Todos;
using Sumodh.Taskora.Test.TestDoubles;

namespace Sumodh.Taskora.Test.Application.Todos;

public class TodoHandlersTests
{
    [Fact]
    public async Task CreateHandle_CreatesTodoForCurrentUser()
    {
        var todoRepository = new FakeTodoRepository();
        var currentUser = new FakeCurrentUserService { UserId = 5 };
        var handler = new CreateTodoCommandHandler(todoRepository, currentUser);
        var dueDate = new DateTime(2026, 3, 24, 0, 0, 0, DateTimeKind.Utc);

        var result = await handler.Handle(
            new CreateTodoCommand("  Finish docs  ", "  Ship tests  ", (int)TodoPriority.High, dueDate),
            CancellationToken.None);

        Assert.NotNull(todoRepository.AddedTodo);
        Assert.Equal(5, todoRepository.AddedTodo!.UserId);
        Assert.Equal("Finish docs", todoRepository.AddedTodo.Title);
        Assert.Equal("Ship tests", todoRepository.AddedTodo.Description);
        Assert.Equal(1, todoRepository.SaveChangesCallCount);
        Assert.Equal("Finish docs", result.Title);
        Assert.Equal((int)TodoPriority.High, result.Priority);
        Assert.Equal(dueDate, result.ToBeCompletedByDateUtc);
    }

    [Fact]
    public async Task UpdateHandle_WithOwnedTodo_UpdatesAndReturnsDto()
    {
        var existingTodo = new TodoItem(5, "Original", TodoPriority.Low, "Old", null);
        var todoRepository = new FakeTodoRepository { TodoByIdForUserResult = existingTodo };
        var currentUser = new FakeCurrentUserService { UserId = 5 };
        var handler = new UpdateTodoCommandHandler(todoRepository, currentUser);
        var dueDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = await handler.Handle(
            new UpdateTodoCommand(9, " Updated ", " New ", (int)TodoPriority.Medium, dueDate),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal((9, 5), todoRepository.LastGetByIdForUserArguments);
        Assert.Equal("Updated", existingTodo.Title);
        Assert.Equal("New", existingTodo.Description);
        Assert.Equal(TodoPriority.Medium, existingTodo.Priority);
        Assert.Equal(1, todoRepository.SaveChangesCallCount);
        Assert.Equal("Updated", result!.Title);
    }

    [Fact]
    public async Task UpdateHandle_WhenTodoIsMissing_ReturnsNull()
    {
        var handler = new UpdateTodoCommandHandler(
            new FakeTodoRepository(),
            new FakeCurrentUserService { UserId = 5 });

        var result = await handler.Handle(
            new UpdateTodoCommand(9, "Updated", null, (int)TodoPriority.Medium, null),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteHandle_WithOwnedTodo_RemovesAndSaves()
    {
        var existingTodo = new TodoItem(5, "Original", TodoPriority.Low, null, null);
        var todoRepository = new FakeTodoRepository { TodoByIdForUserResult = existingTodo };
        var handler = new DeleteTodoCommandHandler(todoRepository, new FakeCurrentUserService { UserId = 5 });

        var deleted = await handler.Handle(new DeleteTodoCommand(3), CancellationToken.None);

        Assert.True(deleted);
        Assert.Same(existingTodo, todoRepository.RemovedTodo);
        Assert.Equal(1, todoRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeleteHandle_WhenTodoIsMissing_ReturnsFalse()
    {
        var todoRepository = new FakeTodoRepository();
        var handler = new DeleteTodoCommandHandler(todoRepository, new FakeCurrentUserService { UserId = 5 });

        var deleted = await handler.Handle(new DeleteTodoCommand(3), CancellationToken.None);

        Assert.False(deleted);
        Assert.Null(todoRepository.RemovedTodo);
        Assert.Equal(0, todoRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task CompleteHandle_WithOwnedTodo_CompletesAndReturnsDto()
    {
        var existingTodo = new TodoItem(5, "Original", TodoPriority.Low, null, null);
        var todoRepository = new FakeTodoRepository { TodoByIdForUserResult = existingTodo };
        var handler = new CompleteTodoCommandHandler(todoRepository, new FakeCurrentUserService { UserId = 5 });

        var result = await handler.Handle(new CompleteTodoCommand(8), CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(existingTodo.IsCompleted);
        Assert.NotNull(existingTodo.CompletedAtUtc);
        Assert.Equal(1, todoRepository.SaveChangesCallCount);
        Assert.True(result!.IsCompleted);
    }

    [Fact]
    public async Task CompleteHandle_WhenTodoIsMissing_ReturnsNull()
    {
        var handler = new CompleteTodoCommandHandler(
            new FakeTodoRepository(),
            new FakeCurrentUserService { UserId = 5 });

        var result = await handler.Handle(new CompleteTodoCommand(8), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ReopenHandle_WithOwnedTodo_ReopensAndReturnsDto()
    {
        var existingTodo = new TodoItem(5, "Original", TodoPriority.Low, null, null);
        existingTodo.MarkComplete();
        var todoRepository = new FakeTodoRepository { TodoByIdForUserResult = existingTodo };
        var handler = new ReopenTodoCommandHandler(todoRepository, new FakeCurrentUserService { UserId = 5 });

        var result = await handler.Handle(new ReopenTodoCommand(8), CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(existingTodo.IsCompleted);
        Assert.Null(existingTodo.CompletedAtUtc);
        Assert.Equal(1, todoRepository.SaveChangesCallCount);
        Assert.False(result!.IsCompleted);
    }

    [Fact]
    public async Task ReopenHandle_WhenTodoIsMissing_ReturnsNull()
    {
        var handler = new ReopenTodoCommandHandler(
            new FakeTodoRepository(),
            new FakeCurrentUserService { UserId = 5 });

        var result = await handler.Handle(new ReopenTodoCommand(8), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdHandle_WithOwnedTodo_ReturnsMappedDto()
    {
        var dueDate = new DateTime(2026, 3, 24, 0, 0, 0, DateTimeKind.Utc);
        var existingTodo = new TodoItem(5, "Original", TodoPriority.High, "Details", dueDate);
        var todoRepository = new FakeTodoRepository { TodoByIdForUserResult = existingTodo };
        var handler = new GetTodoByIdQueryHandler(todoRepository, new FakeCurrentUserService { UserId = 5 });

        var result = await handler.Handle(new GetTodoByIdQuery(11), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal((11, 5), todoRepository.LastGetByIdForUserArguments);
        Assert.Equal("Original", result!.Title);
        Assert.Equal("Details", result.Description);
        Assert.Equal((int)TodoPriority.High, result.Priority);
        Assert.Equal(dueDate, result.ToBeCompletedByDateUtc);
    }

    [Fact]
    public async Task GetByIdHandle_WhenTodoIsMissing_ReturnsNull()
    {
        var handler = new GetTodoByIdQueryHandler(
            new FakeTodoRepository(),
            new FakeCurrentUserService { UserId = 5 });

        var result = await handler.Handle(new GetTodoByIdQuery(11), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMyTodosHandle_ReturnsPagedTodosForCurrentUser()
    {
        var firstTodo = new TodoItem(5, "First", TodoPriority.Low, null, null);
        var secondTodo = new TodoItem(5, "Second", TodoPriority.High, "Details", null);
        secondTodo.MarkComplete();

        var todoRepository = new FakeTodoRepository
        {
            PagedTodosResult = [firstTodo, secondTodo],
            PagedTotalCountResult = 2
        };
        var handler = new GetMyTodosQueryHandler(todoRepository, new FakeCurrentUserService { UserId = 5 });

        var result = await handler.Handle(
            new GetMyTodosQuery(TodoStatusFilter.All, null, null, null, 1, 20),
            CancellationToken.None);

        Assert.Equal((5, null, null, null, null, 1, 20), todoRepository.LastGetPagedForUserArguments);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("First", result.Items[0].Title);
        Assert.Equal("Second", result.Items[1].Title);
        Assert.True(result.Items[1].IsCompleted);
        Assert.Equal((int)TodoPriority.High, result.Items[1].Priority);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task GetMyTodosHandle_NormalizesFiltersAndPagingBeforeCallingRepository()
    {
        var dueBefore = new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc);
        var todoRepository = new FakeTodoRepository
        {
            PagedTotalCountResult = 250
        };
        var handler = new GetMyTodosQueryHandler(todoRepository, new FakeCurrentUserService { UserId = 7 });

        var result = await handler.Handle(
            new GetMyTodosQuery(TodoStatusFilter.Completed, 99, "urgent", dueBefore, 0, 500),
            CancellationToken.None);

        Assert.Equal((7, true, null, "urgent", dueBefore, 1, 100), todoRepository.LastGetPagedForUserArguments);
        Assert.Equal(1, result.Page);
        Assert.Equal(100, result.PageSize);
        Assert.Equal(250, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task GetMyTodosHandle_WhenNoResults_ReturnsEmptyPageWithZeroTotalPages()
    {
        var todoRepository = new FakeTodoRepository
        {
            PagedTodosResult = [],
            PagedTotalCountResult = 0
        };
        var handler = new GetMyTodosQueryHandler(todoRepository, new FakeCurrentUserService { UserId = 5 });

        var result = await handler.Handle(
            new GetMyTodosQuery(TodoStatusFilter.Pending, 2, "docs", null, 2, 10),
            CancellationToken.None);

        Assert.Equal((5, false, 2, "docs", null, 2, 10), todoRepository.LastGetPagedForUserArguments);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalPages);
    }
}
