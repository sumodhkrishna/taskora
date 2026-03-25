using Sumodh.Taskora.Domain.Todos;

namespace Sumodh.Taskora.Application.Abstractions.Persistence
{
    public interface ITodoRepository
    {
        Task AddAsync(TodoItem todoItem, CancellationToken cancellationToken);
        Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<List<TodoItem>> GetByUserIdAsync(int userId, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken); 
        Task<TodoItem?> GetByIdForUserAsync(int id, int userId, CancellationToken cancellationToken);
        void Remove(TodoItem todoItem);

    }
}
