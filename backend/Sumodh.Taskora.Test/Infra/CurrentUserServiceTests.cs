using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Sumodh.Taskora.Infra.Authentication;

namespace Sumodh.Taskora.Test.Infra;

public class CurrentUserServiceTests
{
    [Fact]
    public void UserId_WhenClaimExists_ReturnsParsedValue()
    {
        var service = CreateService(
            new Claim(ClaimTypes.NameIdentifier, "42"),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim(ClaimTypes.Name, "Sumodh"));

        Assert.Equal(42, service.UserId);
        Assert.Equal("user@example.com", service.Email);
        Assert.Equal("Sumodh", service.Name);
    }

    [Fact]
    public void UserId_WhenClaimIsMissing_ThrowsUnauthorizedAccessException()
    {
        var service = CreateService();

        Assert.Throws<UnauthorizedAccessException>(() => _ = service.UserId);
    }

    [Fact]
    public void UserId_WhenClaimIsNotAnInteger_ThrowsUnauthorizedAccessException()
    {
        var service = CreateService(new Claim(ClaimTypes.NameIdentifier, "abc"));

        Assert.Throws<UnauthorizedAccessException>(() => _ = service.UserId);
    }

    private static CurrentUserService CreateService(params Claim[] claims)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
        };

        return new CurrentUserService(new HttpContextAccessor
        {
            HttpContext = context
        });
    }
}
