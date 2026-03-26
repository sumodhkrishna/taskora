using System.IO;
using Microsoft.Extensions.Options;
using Sumodh.Taskora.Infra.Email;

namespace Sumodh.Taskora.Test.Infra;

public class ConsolePasswordResetEmailSenderTests
{
    [Fact]
    public async Task SendAsync_WritesResetDetailsToConsole()
    {
        var options = Options.Create(new SendGridEmailOptions
        {
            ApiKey = "dev-placeholder",
            FromEmail = "verified-sender@example.com",
            FromName = "Taskora Dev",
            PasswordResetUrl = "http://localhost:5173/reset-password",
            EmailVerificationUrl = "http://localhost:5173/verify-email"
        });
        var previewStore = new DevelopmentEmailPreviewStore();
        var sender = new ConsolePasswordResetEmailSender(options, previewStore);
        var originalOut = Console.Out;
        using var writer = new StringWriter();

        Console.SetOut(writer);
        try
        {
            await sender.SendAsync("Sumodh", "user@example.com", "reset-token-123", CancellationToken.None);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        var output = writer.ToString();
        Assert.Contains("Taskora Password Reset Email (Development Mock)", output);
        Assert.Contains("Sumodh <user@example.com>", output);
        Assert.Contains("localhost:5173/reset-password", output);
        Assert.Contains("token=reset-token-123", output);
        Assert.Contains("Reset Token: reset-token-123", output);
        Assert.NotNull(previewStore.GetLatest("password-reset", "user@example.com"));
    }
}
