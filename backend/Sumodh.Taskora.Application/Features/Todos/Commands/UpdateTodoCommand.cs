using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Application.Features.Todos.Commands
{
    public sealed record UpdateTodoCommand(int Id,string Title,string? Description,int Priority,DateTime? ToBeCompletedByDateUtc);
}
