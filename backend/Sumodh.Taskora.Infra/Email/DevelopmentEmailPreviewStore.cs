using Sumodh.Taskora.Application.Abstractions.Communication;
using System.Collections.Concurrent;

namespace Sumodh.Taskora.Infra.Email
{
    public sealed class DevelopmentEmailPreviewStore : IDevelopmentEmailPreviewStore
    {
        private readonly ConcurrentDictionary<string, DevelopmentEmailPreview> _previews = new();

        public void Save(string purpose, DevelopmentEmailPreview preview)
        {
            _previews[CreateKey(purpose, preview.RecipientEmail)] = preview;
        }

        public DevelopmentEmailPreview? GetLatest(string purpose, string recipientEmail)
        {
            _previews.TryGetValue(CreateKey(purpose, recipientEmail), out var preview);
            return preview;
        }

        private static string CreateKey(string purpose, string recipientEmail)
            => $"{purpose}:{recipientEmail.Trim().ToLowerInvariant()}";
    }
}
