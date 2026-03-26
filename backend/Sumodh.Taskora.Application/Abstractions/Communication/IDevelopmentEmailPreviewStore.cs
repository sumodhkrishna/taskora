namespace Sumodh.Taskora.Application.Abstractions.Communication
{
    public interface IDevelopmentEmailPreviewStore
    {
        void Save(string purpose, DevelopmentEmailPreview preview);
        DevelopmentEmailPreview? GetLatest(string purpose, string recipientEmail);
    }
}
