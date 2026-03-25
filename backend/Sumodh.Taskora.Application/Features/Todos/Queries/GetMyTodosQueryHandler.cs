using Sumodh.Taskora.Application.Abstractions.Identity;
using Sumodh.Taskora.Application.Abstractions.Persistence;
using Sumodh.Taskora.Application.Features.Todos.Dtos;

namespace Sumodh.Taskora.Application.Features.Todos.Queries
{
    public sealed class GetMyTodosQueryHandler
    {
        private const int MaxPageSize = 100;

        private readonly ITodoRepository _todoRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetMyTodosQueryHandler(ITodoRepository todoRepository,ICurrentUserService currentUserService)
        {
            _todoRepository = todoRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResultDto<TodoDto>> Handle(GetMyTodosQuery query,CancellationToken cancellationToken)
        {
            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, MaxPageSize);

            bool? isCompleted = query.Status switch
            {
                TodoStatusFilter.Pending => false,
                TodoStatusFilter.Completed => true,
                _ => null
            };

            int? priority = query.Priority is >= 1 and <= 3
                ? query.Priority
                : null;

            var (items, totalCount) = await _todoRepository.GetPagedForUserAsync(_currentUserService.UserId,isCompleted,priority,query.Search,query.DueBefore,page,pageSize,cancellationToken);

            var dtoItems = items.Select(todo => new TodoDto
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

            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResultDto<TodoDto>
            {
                Items = dtoItems,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }
    }
}
