using Sumodh.Taskora.Application.Abstractions.Identity;
using Sumodh.Taskora.Application.Abstractions.Persistence;

namespace Sumodh.Taskora.Application.Features.Todos.Commands
{
    public sealed class DeleteTodoCommandHandler
    {
        private readonly ITodoRepository _todoRepository;
        private readonly ICurrentUserService _currentUserService;

        public DeleteTodoCommandHandler(ITodoRepository todoRepository,ICurrentUserService currentUserService)
        {
            _todoRepository = todoRepository;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(DeleteTodoCommand command, CancellationToken cancellationToken)
        {
            var todo = await _todoRepository.GetByIdForUserAsync(command.Id,_currentUserService.UserId,cancellationToken);

            if (todo is null)
                return false;

            _todoRepository.Remove(todo);
            await _todoRepository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
