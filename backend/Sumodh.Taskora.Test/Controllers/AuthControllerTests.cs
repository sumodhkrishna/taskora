using Microsoft.AspNetCore.Mvc;
using Sumodh.Taskora.Api.Contracts.Auth.Login;
using Sumodh.Taskora.Api.Contracts.Auth.Logout;
using Sumodh.Taskora.Api.Contracts.Auth.RefreshToken;
using Sumodh.Taskora.Api.Contracts.Auth.Register;
using Sumodh.Taskora.Api.Contracts.Auth.ResetPassword;
using Sumodh.Taskora.Api.Contracts.Auth.VerifyEmail;
using Sumodh.Taskora.Api.Controllers;
using Sumodh.Taskora.Application.Features.Auth.Commands.Login;
using Sumodh.Taskora.Application.Features.Auth.Commands.Logout;
using Sumodh.Taskora.Application.Features.Auth.Commands.RefreshToken;
using Sumodh.Taskora.Application.Features.Auth.Commands.Register;
using Sumodh.Taskora.Application.Features.Auth.Commands.RequestPasswordReset;
using Sumodh.Taskora.Application.Features.Auth.Commands.ResendEmailVerification;
using Sumodh.Taskora.Application.Features.Auth.Commands.ResetPassword;
using Sumodh.Taskora.Application.Features.Auth.Commands.VerifyEmail;
using Sumodh.Taskora.Application.Features.Auth.Dtos;
using Sumodh.Taskora.Domain.Authentication;
using Sumodh.Taskora.Domain.Users;
using Sumodh.Taskora.Test.TestDoubles;

namespace Sumodh.Taskora.Test.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ReturnsOkWithEmailActionResponse()
    {
        var controller = new AuthController();
        var handler = new RegisterCommandHandler(
            new FakeUserRepository(),
            new StubPasswordHasher { HashResult = "hashed-password" },
            new StubEmailVerificationTokenGenerator
            {
                GenerateResult = "verify-raw-token",
                HashResult = "verify-hashed-token"
            },
            new StubEmailVerificationEmailSender());

        var result = await controller.Register(
            new RegisterRequest
            {
                Name = "Sumodh",
                Email = "user@example.com",
                Password = "secret123"
            },
            handler,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<EmailActionResponseDto>(okResult.Value);
        Assert.Equal("Account created. Please verify your email before signing in.", response.Message);
        Assert.Equal("user@example.com", response.Email);
    }

    [Fact]
    public async Task Login_ReturnsOkWithAuthResponse()
    {
        var user = new User("Sumodh", "user@example.com", "stored-hash") { Id = 17 };
        user.MarkEmailVerified(DateTime.UtcNow);

        var controller = new AuthController();
        var handler = new LoginCommandHandler(
            new FakeUserRepository { UserByEmailResult = user },
            new StubPasswordHasher { VerifyResult = true },
            new StubJwtTokenGenerator { TokenToReturn = "access-token" },
            new StubRefreshTokenGenerator
            {
                GenerateResult = "refresh-token",
                HashResult = "hashed-refresh-token"
            },
            new FakeRefreshTokenRepository());

        var result = await controller.Login(
            new LoginRequest { Email = "user@example.com", Password = "secret123" },
            handler,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("refresh-token", response.RefreshToken);
        Assert.Equal("user@example.com", response.Email);
    }

    [Fact]
    public async Task VerifyEmail_WhenInvalid_ReturnsBadRequest()
    {
        var user = new User("Sumodh", "user@example.com", "hash");
        user.SetEmailVerificationToken("expected-hash", DateTime.UtcNow.AddHours(1));

        var controller = new AuthController();
        var handler = new VerifyEmailCommandHandler(
            new FakeUserRepository { UserByEmailResult = user },
            new StubEmailVerificationTokenGenerator { HashResult = "wrong-hash" });

        var result = await controller.VerifyEmail(
            new VerifyEmailRequest { Email = "user@example.com", Token = "wrong-token" },
            handler,
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<MessageResponseDto>(badRequest.Value);
        Assert.Equal("Invalid or expired verification token.", response.Message);
    }

    [Fact]
    public async Task RequestPasswordReset_ReturnsOkWithSanitizedEmail()
    {
        var user = new User("Sumodh", "user@example.com", "stored-hash");
        var controller = new AuthController();
        var handler = new RequestPasswordResetCommandHandler(
            new FakeUserRepository { UserByEmailResult = user },
            new StubPasswordResetTokenGenerator
            {
                GenerateResult = "raw-reset-token",
                HashResult = "hashed-reset-token"
            },
            new StubPasswordResetEmailSender());

        var result = await controller.RequestPasswordReset(
            new RequestPasswordResetRequest { Email = " user@example.com " },
            handler,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<EmailActionResponseDto>(okResult.Value);
        Assert.Equal("If the account exists, a password reset email has been sent.", response.Message);
        Assert.Equal("user@example.com", response.Email);
    }

    [Fact]
    public async Task ResetPassword_WhenInvalid_ReturnsBadRequest()
    {
        var controller = new AuthController();
        var handler = new ResetPasswordCommandHandler(
            new FakeUserRepository(),
            new StubPasswordResetTokenGenerator(),
            new StubPasswordHasher());

        var result = await controller.ResetPassword(
            new ResetPasswordRequest
            {
                Email = "user@example.com",
                Token = "bad-token",
                NewPassword = "new-password"
            },
            handler,
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<MessageResponseDto>(badRequest.Value);
        Assert.Equal("Invalid or expired reset token.", response.Message);
    }

    [Fact]
    public async Task Refresh_WhenTokenInvalid_ReturnsUnauthorized()
    {
        var controller = new AuthController();
        var handler = new RefreshTokenCommandHandler(
            new FakeRefreshTokenRepository(),
            new FakeUserRepository(),
            new StubJwtTokenGenerator(),
            new StubRefreshTokenGenerator { HashResult = "hashed-incoming-token" });

        var result = await controller.Refresh(
            new RefreshTokenRequest { RefreshToken = "incoming-token" },
            handler,
            CancellationToken.None);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var response = Assert.IsType<MessageResponseDto>(unauthorized.Value);
        Assert.Equal("Invalid or expired refresh token.", response.Message);
    }

    [Fact]
    public async Task Logout_ReturnsNoContent()
    {
        var controller = new AuthController();
        var handler = new LogoutCommandHandler(new FakeRefreshTokenRepository(), new StubRefreshTokenGenerator());

        var result = await controller.Logout(
            new LogoutRequest { RefreshToken = "refresh-token" },
            handler,
            CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }
}
