namespace Sumodh.Taskora.Application.Features.Auth.Dtos
{
    public sealed class EmailActionResponseDto
    {
        public string Message { get; init; } = string.Empty;
        public string? Email { get; init; }
        public DevelopmentEmailPreviewDto? DevEmailPreview { get; init; }
    }
}
