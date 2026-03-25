using Sumodh.Taskora.Application.Abstractions.Identity;
using Sumodh.Taskora.Application.Abstractions.Persistence;
using Sumodh.Taskora.Application.Features.Todos.Dtos;

namespace Sumodh.Taskora.Application.Features.Todos.Queries
{
    public sealed class GetMyTodosQueryHandler
    {
        private readonly ITodoRepository _todoRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetMyTodosQueryHandler(
            ITodoRepository todoRepository,
            ICurrentUserService currentUserService)
        {
            _todoRepository = todoRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<TodoDto>> Handle(GetMyTodosQuery query, CancellationToken cancellationToken)
        {
            var todos = await _todoRepository.GetByUserIdAsync(_currentUserService.UserId, cancellationToken);

            return todos.Select(todo => new TodoDto
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
            }).ToList();
        }
    }
}
