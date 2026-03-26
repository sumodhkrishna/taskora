using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Sumodh.Taskora.Application.Abstractions.Communication;

namespace Sumodh.Taskora.Infra.Email
{
    public sealed class ConsoleEmailVerificationEmailSender : IEmailVerificationEmailSender
    {
        private readonly SendGridEmailOptions _options;
        private readonly IDevelopmentEmailPreviewStore _previewStore;

        public ConsoleEmailVerificationEmailSender(
            IOptions<SendGridEmailOptions> options,
            IDevelopmentEmailPreviewStore previewStore)
        {
            _options = options.Value;
            _previewStore = previewStore;
        }

        public Task SendAsync(string name, string email, string verificationToken, CancellationToken cancellationToken)
        {
            var actionUrl = QueryHelpers.AddQueryString(_options.EmailVerificationUrl, new Dictionary<string, string?>
            {
                ["email"] = email,
                ["token"] = verificationToken
            });

            _previewStore.Save("email-verification", new DevelopmentEmailPreview
            {
                RecipientEmail = email,
                Subject = "Verify your Taskora email",
                ActionUrl = actionUrl,
                Token = verificationToken
            });

            Console.WriteLine("=== Taskora Email Verification (Development Mock) ===");
            Console.WriteLine($"To: {name} <{email}>");
            Console.WriteLine($"Verification URL: {actionUrl}");
            Console.WriteLine($"Verification Token: {verificationToken}");
            Console.WriteLine("====================================================");

            return Task.CompletedTask;
        }
    }
}
