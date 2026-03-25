using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Sumodh.Taskora.Infra.Authentication;

namespace Sumodh.Taskora.Test.Infra;

public class JWTTokenGeneratorTests
{
    [Fact]
    public void GenerateToken_EmbedsExpectedClaimsAndMetadata()
    {
        var options = Options.Create(new JwtOptions
        {
            Key = "THIS_IS_A_LONG_DEV_ONLY_SECRET_KEY_CHANGE_IT",
            Issuer = "Taskora",
            Audience = "Taskora.UI",
            ExpiryMinutes = 60
        });
        var generator = new JwtTokenGenerator(options);
        var before = DateTime.UtcNow;

        var token = generator.GenerateToken(17, "user@example.com", "Sumodh");

        var after = DateTime.UtcNow;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("Taskora", jwt.Issuer);
        Assert.Equal("Taskora.UI", jwt.Audiences.Single());
        Assert.Equal("17", jwt.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("17", jwt.Claims.Single(claim => claim.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value);
        Assert.Equal("user@example.com", jwt.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("user@example.com", jwt.Claims.Single(claim => claim.Type == System.Security.Claims.ClaimTypes.Email).Value);
        Assert.Equal("Sumodh", jwt.Claims.Single(claim => claim.Type == System.Security.Claims.ClaimTypes.Name).Value);
        Assert.InRange(jwt.ValidTo, before.AddMinutes(59), after.AddMinutes(61));
    }
}
