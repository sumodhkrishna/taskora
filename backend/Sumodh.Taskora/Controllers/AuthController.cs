using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Sumodh.Taskora.Api.Contracts.Auth.Login;
using Sumodh.Taskora.Api.Contracts.Auth.Logout;
using Sumodh.Taskora.Api.Contracts.Auth.RefreshToken;
using Sumodh.Taskora.Api.Contracts.Auth.Register;
using Sumodh.Taskora.Api.Contracts.Auth.ResetPassword;
using Sumodh.Taskora.Api.Contracts.Users;
using Sumodh.Taskora.Application.Features.Auth.Commands.Login;
using Sumodh.Taskora.Application.Features.Auth.Commands.Logout;
using Sumodh.Taskora.Application.Features.Auth.Commands.RefreshToken;
using Sumodh.Taskora.Application.Features.Auth.Commands.Register;
using Sumodh.Taskora.Application.Features.Auth.Commands.RequestPasswordReset;
using Sumodh.Taskora.Application.Features.Auth.Commands.ResetPassword;
using Sumodh.Taskora.Application.Features.Auth.Dtos;

namespace Sumodh.Taskora.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [EnableRateLimiting("AuthSensitivePolicy")]
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request, [FromServices] RegisterCommandHandler handler, CancellationToken cancellationToken)
        {
            var command = new RegisterCommand(request.Name, request.Email, request.Password);
            var user = await handler.Handle(command, cancellationToken);
            return CreatedAtAction("GetUserById", controllerName: "Users", new { id = user.Id }, user);
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitivePolicy")]
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequest request, [FromServices] LoginCommandHandler handler, CancellationToken cancellationToken)
        {
            var result = await handler.Handle(new LoginCommand(request.Email, request.Password), cancellationToken);
            return Ok(result);
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitivePolicy")]
        [HttpPost("password-reset/request")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request, [FromServices] RequestPasswordResetCommandHandler handler, CancellationToken cancellationToken)
        {
            await handler.Handle(new RequestPasswordResetCommand(request.Email), cancellationToken);
            return Ok(new
            {
                message = "If the account exists, a password reset email has been sent."
            });
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitivePolicy")]
        [HttpPost("password-reset/confirm")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, [FromServices] ResetPasswordCommandHandler handler, CancellationToken cancellationToken)
        {
            var success = await handler.Handle(new ResetPasswordCommand(request.Email, request.Token, request.NewPassword), cancellationToken);
            if (!success)
                return BadRequest(new { message = "Invalid or expired reset token." });
            return Ok(new { message = "Password has been reset successfully." });
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSessionPolicy")]
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequest request,[FromServices] RefreshTokenCommandHandler handler,CancellationToken cancellationToken)
        {
            var result = await handler.Handle(new RefreshTokenCommand(request.RefreshToken),cancellationToken);
            if (result is null)
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            return Ok(result);
        }

        [EnableRateLimiting("AuthSessionPolicy")]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request,[FromServices] LogoutCommandHandler handler,CancellationToken cancellationToken)
        {
            await handler.Handle(new LogoutCommand(request.RefreshToken),cancellationToken);
            return NoContent();
        }
    }
}
