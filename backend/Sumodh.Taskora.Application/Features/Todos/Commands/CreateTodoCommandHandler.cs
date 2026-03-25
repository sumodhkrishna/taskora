using Sumodh.Taskora.Application.Abstractions.Identity;
using Sumodh.Taskora.Application.Abstractions.Persistence;
using Sumodh.Taskora.Application.Features.Todos.Dtos;
using Sumodh.Taskora.Domain.Todos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Application.Features.Todos.Commands
{
    public class CreateTodoCommandHandler
    {
        private readonly ITodoRepository _todoRepository;
        private readonly ICurrentUserService _currentUserService;

        public CreateTodoCommandHandler(ITodoRepository todoRepository,ICurrentUserService currentUserService)
        {
            _todoRepository = todoRepository;
            _currentUserService = currentUserService;
        }

        public async Task<TodoDto> Handle(CreateTodoCommand command, CancellationToken cancellationToken)
        {
            var todoItem = new TodoItem(_currentUserService.UserId,command.Title,(TodoPriority)command.Priority,command.Description,command.ToBeCompletedByDateUtc);

            await _todoRepository.AddAsync(todoItem, cancellationToken);
            await _todoRepository.SaveChangesAsync(cancellationToken);

            return new TodoDto
            {
                Id = todoItem.Id,
                Title = todoItem.Title,
                Description = todoItem.Description,
                Priority = (int)todoItem.Priority,
                ToBeCompletedByDateUtc = todoItem.ToBeCompletedByDateUtc,
                IsCompleted = todoItem.IsCompleted,
                CreatedAtUtc = todoItem.CreatedAtUtc,
                UpdatedAtUtc = todoItem.UpdatedAtUtc,
                CompletedAtUtc = todoItem.CompletedAtUtc
            };
        }
    }
}
