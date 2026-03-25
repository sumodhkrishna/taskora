using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Domain.Todos
{
    public class TodoItem
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }

        public string Title { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public TodoPriority Priority { get; private set; }
        public DateTime? ToBeCompletedByDateUtc { get; private set; }

        public bool IsCompleted { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public DateTime? CompletedAtUtc { get; private set; }

        private TodoItem()
        {
        }

        public TodoItem(int userId,string title,TodoPriority priority,string? description,DateTime? toBeCompletedByDateUtc)
        {
            if (userId <= 0)
                throw new ArgumentException("User is required.", nameof(userId));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.", nameof(title));

            if (!Enum.IsDefined(typeof(TodoPriority), priority))
                throw new ArgumentException("Invalid priority.", nameof(priority));

            UserId = userId;
            Title = title.Trim();
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            Priority = priority;
            ToBeCompletedByDateUtc = toBeCompletedByDateUtc;

            IsCompleted = false;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public void Update(string title,TodoPriority priority,string? description,DateTime? toBeCompletedByDateUtc)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.", nameof(title));

            if (!Enum.IsDefined(typeof(TodoPriority), priority))
                throw new ArgumentException("Invalid priority.", nameof(priority));

            Title = title.Trim();
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            Priority = priority;
            ToBeCompletedByDateUtc = toBeCompletedByDateUtc;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void MarkComplete()
        {
            if (IsCompleted)
                return;

            IsCompleted = true;
            CompletedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void MarkIncomplete()
        {
            if (!IsCompleted)
                return;

            IsCompleted = false;
            CompletedAtUtc = null;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
