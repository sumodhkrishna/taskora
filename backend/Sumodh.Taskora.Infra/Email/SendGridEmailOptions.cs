using System.ComponentModel.DataAnnotations;

namespace Sumodh.Taskora.Infra.Email
{
    public sealed class SendGridEmailOptions
    {
        public const string SectionName = "SendGrid";

        [Required]
        public string ApiKey { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        public string FromEmail { get; init; } = string.Empty;

        [Required]
        public string FromName { get; init; } = string.Empty;

        [Required]
        [Url]
        public string PasswordResetUrl { get; init; } = string.Empty;
    }
}
