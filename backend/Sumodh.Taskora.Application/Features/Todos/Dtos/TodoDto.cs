using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Application.Features.Todos.Dtos
{
    public class TodoDto
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public int Priority { get; init; }
        public DateTime? ToBeCompletedByDateUtc { get; init; }
        public bool IsCompleted { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
    }
}
