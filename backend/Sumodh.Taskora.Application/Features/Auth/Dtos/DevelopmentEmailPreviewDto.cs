namespace Sumodh.Taskora.Application.Features.Auth.Dtos
{
    public sealed class DevelopmentEmailPreviewDto
    {
        public string RecipientEmail { get; init; } = string.Empty;
        public string Subject { get; init; } = string.Empty;
        public string ActionUrl { get; init; } = string.Empty;
        public string Token { get; init; } = string.Empty;
    }
}
