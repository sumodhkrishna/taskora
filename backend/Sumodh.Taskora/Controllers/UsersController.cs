using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sumodh.Taskora.Api.Contracts.Users;
using Sumodh.Taskora.Application.Features.Users.Queries.GetById;
using Sumodh.Taskora.Application.Features.Users.Queries.GetCurrentUser;

namespace Sumodh.Taskora.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [Authorize]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetUserById(int id,[FromServices] GetUserByIdQueryHandler handler,CancellationToken cancellationToken)
        {
            var result = await handler.Handle(new GetUserByIdQuery(id), cancellationToken);
            if (result is null)
                return NotFound();
            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetCurentUser([FromServices] GetCurrentUserQueryHandler handler, CancellationToken cancellationToken)
        {
            var result = await handler.Handle(new GetCurrentUserQuery(), cancellationToken);
            if (result is null)
                return NotFound();
            return Ok(result);
        }
    }
}
