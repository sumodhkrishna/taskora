using Sumodh.Taskora.Application.Abstractions.Identity;
using Sumodh.Taskora.Application.Abstractions.Persistence;
using Sumodh.Taskora.Application.Features.Todos.Dtos;
using Sumodh.Taskora.Domain.Todos;

namespace Sumodh.Taskora.Application.Features.Todos.Commands
{
    public sealed class UpdateTodoCommandHandler
    {
        private readonly ITodoRepository _todoRepository;
        private readonly ICurrentUserService _currentUserService;

        public UpdateTodoCommandHandler(ITodoRepository todoRepository,ICurrentUserService currentUserService)
        {
            _todoRepository = todoRepository;
            _currentUserService = currentUserService;
        }

        public async Task<TodoDto?> Handle(UpdateTodoCommand command, CancellationToken cancellationToken)
        {
            var todo = await _todoRepository.GetByIdForUserAsync(command.Id,_currentUserService.UserId,cancellationToken);

            if (todo is null)
                return null;

            todo.Update(command.Title,(TodoPriority)command.Priority,command.Description,command.ToBeCompletedByDateUtc);

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
