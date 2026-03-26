using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sumodh.Taskora.Application.Features.Auth.Exceptions;
using Sumodh.Taskora.Infrastructure;

namespace Sumodh.Taskora.Test.Infrastructure;

public class GlobalExceptionHandlerTests
{
    [Theory]
    [InlineData(typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized, "Unauthorized", "nope")]
    [InlineData(typeof(EmailNotVerifiedException), StatusCodes.Status403Forbidden, "Forbidden", "Please verify your email address before signing in.")]
    [InlineData(typeof(InvalidOperationException), StatusCodes.Status409Conflict, "Conflict", "duplicate")]
    [InlineData(typeof(ArgumentException), StatusCodes.Status400BadRequest, "Bad Request", "bad input")]
    [InlineData(typeof(InvalidDataException), StatusCodes.Status400BadRequest, "Bad Request", "bad file")]
    public async Task TryHandleAsync_KnownExceptions_MapExpectedProblemDetails(
        Type exceptionType,
        int expectedStatus,
        string expectedTitle,
        string message)
    {
        var problemDetailsService = new FakeProblemDetailsService();
        var handler = new GlobalExceptionHandler(problemDetailsService);
        var httpContext = new DefaultHttpContext();
        var exception = exceptionType == typeof(EmailNotVerifiedException)
            ? new EmailNotVerifiedException()
            : (Exception)Activator.CreateInstance(exceptionType, message)!;

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(expectedStatus, httpContext.Response.StatusCode);
        Assert.NotNull(problemDetailsService.LastContext);
        Assert.Equal(expectedStatus, problemDetailsService.LastContext!.ProblemDetails.Status);
        Assert.Equal(expectedTitle, problemDetailsService.LastContext.ProblemDetails.Title);
        Assert.Equal(message, problemDetailsService.LastContext.ProblemDetails.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_UnknownException_ReturnsGenericServerProblem()
    {
        var problemDetailsService = new FakeProblemDetailsService();
        var handler = new GlobalExceptionHandler(problemDetailsService);
        var httpContext = new DefaultHttpContext();

        var handled = await handler.TryHandleAsync(httpContext, new Exception("secret"), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        Assert.NotNull(problemDetailsService.LastContext);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetailsService.LastContext!.ProblemDetails.Status);
        Assert.Equal("Server Error", problemDetailsService.LastContext.ProblemDetails.Title);
        Assert.Equal("An unexpected error occurred.", problemDetailsService.LastContext.ProblemDetails.Detail);
    }

    private sealed class FakeProblemDetailsService : IProblemDetailsService
    {
        public ProblemDetailsContext? LastContext { get; private set; }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            LastContext = context;
            return ValueTask.FromResult(true);
        }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            LastContext = context;
            return ValueTask.CompletedTask;
        }
    }
}
