using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Application.Features.Todos.Dtos
{
    public class PagedResultDto<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
    }
}
