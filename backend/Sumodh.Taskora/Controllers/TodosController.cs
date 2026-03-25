using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sumodh.Taskora.Api.Contracts.Todos;
using Sumodh.Taskora.Application.Features.Todos.Commands;
using Sumodh.Taskora.Application.Features.Todos.Dtos;
using Sumodh.Taskora.Application.Features.Todos.Queries;

namespace Sumodh.Taskora.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodosController : ControllerBase
    {
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<TodoDto>> Create([FromBody] CreateTodoRequest request,[FromServices] CreateTodoCommandHandler handler,CancellationToken cancellationToken)
        {
            var result = await handler.Handle(
                new CreateTodoCommand(request.Title,request.Description,request.Priority,request.ToBeCompletedByDateUtc),cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<TodoDto>>> GetMine([FromQuery] GetMyTodosRequest request,[FromServices] GetMyTodosQueryHandler handler,CancellationToken cancellationToken)
        {
            var status = request.Status?.Trim().ToLowerInvariant() switch
            {
                "pending" => TodoStatusFilter.Pending,
                "completed" => TodoStatusFilter.Completed,
                "all" or null or "" => TodoStatusFilter.All,
                _ => TodoStatusFilter.All
            };

            var query = new GetMyTodosQuery(status,request.Priority,request.Search,request.DueBefore,request.Page,request.PageSize);
            var result = await handler.Handle(query, cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TodoDto>> GetById(int id,[FromServices] GetTodoByIdQueryHandler handler,CancellationToken cancellationToken)
        {
            var result = await handler.Handle(new GetTodoByIdQuery(id), cancellationToken);
            if (result is null)
                return NotFound();
            return Ok(result);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<TodoDto>> Update(int id,[FromBody] UpdateTodoRequest request,[FromServices] UpdateTodoCommandHandler handler,CancellationToken cancellationToken)
        {
            var result = await handler.Handle(new UpdateTodoCommand(id,request.Title,request.Description,request.Priority,request.ToBeCompletedByDateUtc),cancellationToken);
            if (result is null)
                return NotFound();
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id,[FromServices] DeleteTodoCommandHandler handler,CancellationToken cancellationToken)
        {
            var deleted = await handler.Handle(new DeleteTodoCommand(id), cancellationToken);
            if (!deleted)
                return NotFound();
            return NoContent();
        }

        [Authorize]
        [HttpPatch("{id:int}/complete")]
        public async Task<ActionResult<TodoDto>> Complete(int id,[FromServices] CompleteTodoCommandHandler handler,CancellationToken cancellationToken)
        {
            var result = await handler.Handle(new CompleteTodoCommand(id), cancellationToken);
            if (result is null)
                return NotFound();
            return Ok(result);
        }

        [Authorize]
        [HttpPatch("{id:int}/reopen")]
        public async Task<ActionResult<TodoDto>> Reopen(int id,[FromServices] ReopenTodoCommandHandler handler,CancellationToken cancellationToken)
        {
            var result = await handler.Handle(new ReopenTodoCommand(id), cancellationToken);
            if (result is null)
                return NotFound();
            return Ok(result);
        }
    }
}
