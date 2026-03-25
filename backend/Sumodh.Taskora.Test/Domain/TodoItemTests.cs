using Sumodh.Taskora.Domain.Todos;

namespace Sumodh.Taskora.Test.Domain;

public class TodoItemTests
{
    [Fact]
    public void Constructor_WithValidValues_SetsProperties()
    {
        var dueDate = new DateTime(2026, 3, 24, 0, 0, 0, DateTimeKind.Utc);
        var before = DateTime.UtcNow;

        var todo = new TodoItem(7, "  Finish tests  ", TodoPriority.High, "  cover edge cases  ", dueDate);

        var after = DateTime.UtcNow;

        Assert.Equal(7, todo.UserId);
        Assert.Equal("Finish tests", todo.Title);
        Assert.Equal("cover edge cases", todo.Description);
        Assert.Equal(TodoPriority.High, todo.Priority);
        Assert.Equal(dueDate, todo.ToBeCompletedByDateUtc);
        Assert.False(todo.IsCompleted);
        Assert.Null(todo.UpdatedAtUtc);
        Assert.Null(todo.CompletedAtUtc);
        Assert.InRange(todo.CreatedAtUtc, before, after);
    }

    [Fact]
    public void Constructor_WithWhitespaceDescription_NormalizesToNull()
    {
        var todo = new TodoItem(7, "Title", TodoPriority.Medium, "   ", null);

        Assert.Null(todo.Description);
    }

    [Fact]
    public void Constructor_WithInvalidUserId_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => new TodoItem(0, "Title", TodoPriority.Low, null, null));

        Assert.Equal("userId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithBlankTitle_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => new TodoItem(1, " ", TodoPriority.Low, null, null));

        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidPriority_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => new TodoItem(1, "Title", (TodoPriority)99, null, null));

        Assert.Equal("priority", exception.ParamName);
    }

    [Fact]
    public void Update_WithValidValues_UpdatesMutableFields()
    {
        var todo = new TodoItem(1, "Original", TodoPriority.Low, "Old", null);
        var dueDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        todo.Update("  Updated  ", TodoPriority.High, "  New text  ", dueDate);

        Assert.Equal("Updated", todo.Title);
        Assert.Equal("New text", todo.Description);
        Assert.Equal(TodoPriority.High, todo.Priority);
        Assert.Equal(dueDate, todo.ToBeCompletedByDateUtc);
        Assert.NotNull(todo.UpdatedAtUtc);
    }

    [Fact]
    public void Update_WithWhitespaceDescription_ClearsDescription()
    {
        var todo = new TodoItem(1, "Original", TodoPriority.Low, "Old", null);

        todo.Update("Updated", TodoPriority.Medium, "   ", null);

        Assert.Null(todo.Description);
    }

    [Fact]
    public void Update_WithBlankTitle_Throws()
    {
        var todo = new TodoItem(1, "Original", TodoPriority.Low, null, null);

        var exception = Assert.Throws<ArgumentException>(() => todo.Update(" ", TodoPriority.Low, null, null));

        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void Update_WithInvalidPriority_Throws()
    {
        var todo = new TodoItem(1, "Original", TodoPriority.Low, null, null);

        var exception = Assert.Throws<ArgumentException>(() => todo.Update("Updated", (TodoPriority)44, null, null));

        Assert.Equal("priority", exception.ParamName);
    }

    [Fact]
    public void MarkComplete_WhenPending_CompletesTodo()
    {
        var todo = new TodoItem(1, "Original", TodoPriority.Low, null, null);

        todo.MarkComplete();

        Assert.True(todo.IsCompleted);
        Assert.NotNull(todo.CompletedAtUtc);
        Assert.NotNull(todo.UpdatedAtUtc);
    }

    [Fact]
    public void MarkComplete_WhenAlreadyCompleted_DoesNotChangeCompletionTimestamp()
    {
        var todo = new TodoItem(1, "Original", TodoPriority.Low, null, null);
        todo.MarkComplete();
        var completedAt = todo.CompletedAtUtc;
        var updatedAt = todo.UpdatedAtUtc;

        todo.MarkComplete();

        Assert.Equal(completedAt, todo.CompletedAtUtc);
        Assert.Equal(updatedAt, todo.UpdatedAtUtc);
    }

    [Fact]
    public void MarkIncomplete_WhenCompleted_ReopensTodo()
    {
        var todo = new TodoItem(1, "Original", TodoPriority.Low, null, null);
        todo.MarkComplete();

        todo.MarkIncomplete();

        Assert.False(todo.IsCompleted);
        Assert.Null(todo.CompletedAtUtc);
        Assert.NotNull(todo.UpdatedAtUtc);
    }

    [Fact]
    public void MarkIncomplete_WhenAlreadyPending_DoesNothing()
    {
        var todo = new TodoItem(1, "Original", TodoPriority.Low, null, null);

        todo.MarkIncomplete();

        Assert.False(todo.IsCompleted);
        Assert.Null(todo.CompletedAtUtc);
        Assert.Null(todo.UpdatedAtUtc);
    }
}
