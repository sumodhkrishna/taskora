using Microsoft.EntityFrameworkCore;
using Sumodh.Taskora.Application.Abstractions.Persistence;
using Sumodh.Taskora.Domain.Todos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Infra.Persistance.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly AppDbContext _dbContext;

        public TodoRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(TodoItem todoItem, CancellationToken cancellationToken)
        {
            await _dbContext.TodoItems.AddAsync(todoItem, cancellationToken);
        }

        public async Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _dbContext.TodoItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<TodoItem>> GetByUserIdAsync(int userId, CancellationToken cancellationToken)
        {
            return await _dbContext.TodoItems
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.IsCompleted)
                .ThenByDescending(x => x.Priority)
                .ThenBy(x => x.ToBeCompletedByDateUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<TodoItem?> GetByIdForUserAsync(int id, int userId, CancellationToken cancellationToken)
        {
            return await _dbContext.TodoItems
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        }
        public void Remove(TodoItem todoItem)
        {
            _dbContext.TodoItems.Remove(todoItem);
        }
        public async Task<(List<TodoItem> Items, int TotalCount)> GetPagedForUserAsync(int userId,bool? isCompleted,int? priority,string? search,DateTime? dueBefore,int page,int pageSize,CancellationToken cancellationToken)
        {
            IQueryable<TodoItem> query = _dbContext.TodoItems
                .AsNoTracking()
                .Where(x => x.UserId == userId);

            if (isCompleted.HasValue)
            {
                query = query.Where(x => x.IsCompleted == isCompleted.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(x => (int)x.Priority == priority.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();

                query = query.Where(x =>
                    x.Title.Contains(term) ||
                    (x.Description != null && x.Description.Contains(term)));
            }

            if (dueBefore.HasValue)
            {
                query = query.Where(x =>
                    x.ToBeCompletedByDateUtc.HasValue &&
                    x.ToBeCompletedByDateUtc.Value < dueBefore.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.IsCompleted)
                .ThenByDescending(x => x.Priority)
                .ThenBy(x => x.ToBeCompletedByDateUtc)
                .ThenByDescending(x => x.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

    }
}
