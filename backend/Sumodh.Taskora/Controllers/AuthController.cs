using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Sumodh.Taskora.Api.Contracts.Auth.Login;
using Sumodh.Taskora.Api.Contracts.Auth.Logout;
using Sumodh.Taskora.Api.Contracts.Auth.RefreshToken;
using Sumodh.Taskora.Api.Contracts.Auth.Register;
using Sumodh.Taskora.Api.Contracts.Auth.ResetPassword;
using Sumodh.Taskora.Api.Contracts.Auth.VerifyEmail;
using Sumodh.Taskora.Application.Abstractions.Communication;
using Sumodh.Taskora.Application.Features.Auth.Commands.Login;
using Sumodh.Taskora.Application.Features.Auth.Commands.Logout;
using Sumodh.Taskora.Application.Features.Auth.Commands.RefreshToken;
using Sumodh.Taskora.Application.Features.Auth.Commands.Register;
using Sumodh.Taskora.Application.Features.Auth.Commands.RequestPasswordReset;
using Sumodh.Taskora.Application.Features.Auth.Commands.ResendEmailVerification;
using Sumodh.Taskora.Application.Features.Auth.Commands.ResetPassword;
using Sumodh.Taskora.Application.Features.Auth.Commands.VerifyEmail;
using Sumodh.Taskora.Application.Features.Auth.Dtos;
using Sumodh.Taskora.Api.Contracts.Users;

namespace Sumodh.Taskora.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [EnableRateLimiting("AuthSensitivePolicy")]
        [HttpPost("register")]
        [ProducesResponseType(typeof(EmailActionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EmailActionResponseDto>> Register(
            [FromBody] RegisterRequest request,
            [FromServices] RegisterCommandHandler handler,
            CancellationToken cancellationToken)
        {
            var user = await handler.Handle(new RegisterCommand(request.Name, request.Email, request.Password), cancellationToken);
            return Ok(new EmailActionResponseDto
            {
                Message = "Account created. Please verify your email before signing in.",
                Email = user.Email
            });
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitivePolicy")]
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequest request, [FromServices] LoginCommandHandler handler, CancellationToken cancellationToken)
        {
            var result = await handler.Handle(new LoginCommand(request.Email, request.Password), cancellationToken);
            return Ok(result);
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitivePolicy")]
        [HttpPost("verify-email")]
        public async Task<ActionResult<MessageResponseDto>> VerifyEmail(
            [FromBody] VerifyEmailRequest request,
            [FromServices] VerifyEmailCommandHandler handler,
            CancellationToken cancellationToken)
        {
            var verified = await handler.Handle(new VerifyEmailCommand(request.Email, request.Token), cancellationToken);
            if (!verified)
                return BadRequest(new MessageResponseDto { Message = "Invalid or expired verification token." });

            return Ok(new MessageResponseDto { Message = "Your email has been verified successfully." });
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitivePolicy")]
        [HttpPost("verify-email/resend")]
        public async Task<ActionResult<EmailActionResponseDto>> ResendVerificationEmail(
            [FromBody] ResendEmailVerificationRequest request,
            [FromServices] ResendEmailVerificationCommandHandler handler,
            CancellationToken cancellationToken)
        {
            await handler.Handle(new ResendEmailVerificationCommand(request.Email), cancellationToken);
            return Ok(new EmailActionResponseDto
            {
                Message = "If the account exists and is not yet verified, a verification email has been sent.",
                Email = request.Email.Trim()
            });
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitivePolicy")]
        [HttpPost("password-reset/request")]
        public async Task<ActionResult<EmailActionResponseDto>> RequestPasswordReset(
            [FromBody] RequestPasswordResetRequest request,
            [FromServices] RequestPasswordResetCommandHandler handler,
            CancellationToken cancellationToken)
        {
            await handler.Handle(new RequestPasswordResetCommand(request.Email), cancellationToken);
            return Ok(new EmailActionResponseDto
            {
                Message = "If the account exists, a password reset email has been sent.",
                Email = request.Email.Trim()
            });
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitivePolicy")]
        [HttpPost("password-reset/confirm")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, [FromServices] ResetPasswordCommandHandler handler, CancellationToken cancellationToken)
        {
            var success = await handler.Handle(new ResetPasswordCommand(request.Email, request.Token, request.NewPassword), cancellationToken);
            if (!success)
                return BadRequest(new MessageResponseDto { Message = "Invalid or expired reset token." });
            return Ok(new MessageResponseDto { Message = "Password has been reset successfully." });
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSessionPolicy")]
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequest request,[FromServices] RefreshTokenCommandHandler handler,CancellationToken cancellationToken)
        {
            var result = await handler.Handle(new RefreshTokenCommand(request.RefreshToken),cancellationToken);
            if (result is null)
                return Unauthorized(new MessageResponseDto { Message = "Invalid or expired refresh token." });
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
