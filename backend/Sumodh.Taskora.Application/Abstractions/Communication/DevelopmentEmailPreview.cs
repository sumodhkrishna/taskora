namespace Sumodh.Taskora.Application.Abstractions.Communication
{
    public sealed class DevelopmentEmailPreview
    {
        public string RecipientEmail { get; init; } = string.Empty;
        public string Subject { get; init; } = string.Empty;
        public string ActionUrl { get; init; } = string.Empty;
        public string Token { get; init; } = string.Empty;
    }
}
