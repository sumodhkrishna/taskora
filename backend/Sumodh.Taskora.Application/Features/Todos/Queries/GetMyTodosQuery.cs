using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Application.Features.Todos.Queries
{
    public sealed record GetMyTodosQuery(TodoStatusFilter Status,int? Priority,string? Search,DateTime? DueBefore,int Page,int PageSize);
}
