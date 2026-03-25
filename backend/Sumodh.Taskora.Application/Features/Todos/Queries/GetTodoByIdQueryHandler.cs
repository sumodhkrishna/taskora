using Sumodh.Taskora.Application.Abstractions.Identity;
using Sumodh.Taskora.Application.Abstractions.Persistence;
using Sumodh.Taskora.Application.Features.Todos.Dtos;
using Sumodh.Taskora.Domain.Todos;

namespace Sumodh.Taskora.Application.Features.Todos.Queries
{
    public sealed class GetTodoByIdQueryHandler
    {
        private readonly ITodoRepository _todoRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetTodoByIdQueryHandler(
            ITodoRepository todoRepository,
            ICurrentUserService currentUserService)
        {
            _todoRepository = todoRepository;
            _currentUserService = currentUserService;
        }

        public async Task<TodoDto?> Handle(GetTodoByIdQuery query, CancellationToken cancellationToken)
        {
            var todo = await _todoRepository.GetByIdForUserAsync(
                query.Id,
                _currentUserService.UserId,
                cancellationToken);

            if (todo is null)
                return null;

            return Map(todo);
        }

        private static TodoDto Map(TodoItem todo) => new()
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            Priority = (int)todo.Priority,
            ToBeCompletedByDateUtc = todo.ToBeCompletedByDateUtc,
            IsCompleted = todo.IsCompleted,
            CreatedAtUtc = todo.CreatedAtUtc,
            UpdatedAtUtc = todo.UpdatedAtUtc,
            CompletedAtUtc = todo.CompletedAtUtc
        };
    }
}
