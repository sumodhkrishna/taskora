using Sumodh.Taskora.Application.Abstractions.Identity;
using Sumodh.Taskora.Application.Abstractions.Persistence;
using Sumodh.Taskora.Application.Features.Todos.Dtos;

namespace Sumodh.Taskora.Application.Features.Todos.Commands
{
    public sealed class CompleteTodoCommandHandler
    {
        private readonly ITodoRepository _todoRepository;
        private readonly ICurrentUserService _currentUserService;

        public CompleteTodoCommandHandler(ITodoRepository todoRepository,ICurrentUserService currentUserService)
        {
            _todoRepository = todoRepository;
            _currentUserService = currentUserService;
        }

        public async Task<TodoDto?> Handle(CompleteTodoCommand command, CancellationToken cancellationToken)
        {
            var todo = await _todoRepository.GetByIdForUserAsync(command.Id,_currentUserService.UserId,cancellationToken);

            if (todo is null)
                return null;

            todo.MarkComplete();
            await _todoRepository.SaveChangesAsync(cancellationToken);

            return new TodoDto
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
}
