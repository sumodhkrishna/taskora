using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Sumodh.Taskora.Application.Abstractions.Communication;

namespace Sumodh.Taskora.Infra.Email
{
    public sealed class ConsolePasswordResetEmailSender : IPasswordResetEmailSender
    {
        private readonly SendGridEmailOptions _options;
        private readonly IDevelopmentEmailPreviewStore _previewStore;

        public ConsolePasswordResetEmailSender(
            IOptions<SendGridEmailOptions> options,
            IDevelopmentEmailPreviewStore previewStore)
        {
            _options = options.Value;
            _previewStore = previewStore;
        }

        public Task SendAsync(string name, string email, string resetToken, CancellationToken cancellationToken)
        {
            var resetUrl = QueryHelpers.AddQueryString(_options.PasswordResetUrl, new Dictionary<string, string?>
            {
                ["email"] = email,
                ["token"] = resetToken
            });

            _previewStore.Save("password-reset", new DevelopmentEmailPreview
            {
                RecipientEmail = email,
                Subject = "Reset your Taskora password",
                ActionUrl = resetUrl,
                Token = resetToken
            });

            Console.WriteLine("=== Taskora Password Reset Email (Development Mock) ===");
            Console.WriteLine($"To: {name} <{email}>");
            Console.WriteLine($"Reset URL: {resetUrl}");
            Console.WriteLine($"Reset Token: {resetToken}");
            Console.WriteLine("======================================================");

            return Task.CompletedTask;
        }
    }
}
