using System.IO;
using Microsoft.Extensions.Options;
using Sumodh.Taskora.Infra.Email;

namespace Sumodh.Taskora.Test.Infra;

public class ConsoleEmailVerificationEmailSenderTests
{
    [Fact]
    public async Task SendAsync_WritesVerificationDetailsToConsole()
    {
        var options = Options.Create(new SendGridEmailOptions
        {
            ApiKey = "dev-placeholder",
            FromEmail = "verified-sender@example.com",
            FromName = "Taskora Dev",
            PasswordResetUrl = "http://localhost:5173/reset-password",
            EmailVerificationUrl = "http://localhost:5173/verify-email"
        });
        var sender = new ConsoleEmailVerificationEmailSender(options);
        var originalOut = Console.Out;
        using var writer = new StringWriter();

        Console.SetOut(writer);
        try
        {
            await sender.SendAsync("Sumodh", "user@example.com", "verify-token-123", CancellationToken.None);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        var output = writer.ToString();
        Assert.Contains("Taskora Email Verification (Development Mock)", output);
        Assert.Contains("Sumodh <user@example.com>", output);
        Assert.Contains("localhost:5173/verify-email", output);
        Assert.Contains("token=verify-token-123", output);
        Assert.Contains("Verification Token: verify-token-123", output);
    }
}
