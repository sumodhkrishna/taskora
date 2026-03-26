using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Sumodh.Taskora.Infra.Email;

namespace Sumodh.Taskora.Test.Infra;

public class SendGridEmailVerificationEmailSenderTests
{
    [Fact]
    public async Task SendAsync_BuildsSendGridRequestWithVerificationLinkAndToken()
    {
        var handler = new RecordingHttpMessageHandler();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.sendgrid.com/v3/")
        };
        var options = Options.Create(new SendGridEmailOptions
        {
            ApiKey = "test-api-key",
            FromEmail = "verified-sender@example.com",
            FromName = "Taskora",
            PasswordResetUrl = "http://localhost:5173/reset-password",
            EmailVerificationUrl = "http://localhost:5173/verify-email"
        });
        var sender = new SendGridEmailVerificationEmailSender(httpClient, options);

        await sender.SendAsync("Sumodh", "user@example.com", "verify-token-123", CancellationToken.None);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("https://api.sendgrid.com/v3/mail/send", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal("test-api-key", handler.LastRequest.Headers.Authorization.Parameter);

        var body = handler.LastRequestBody!;
        Assert.Contains("\"subject\":\"Verify your Taskora email\"", body);
        Assert.Contains("verified-sender@example.com", body);
        Assert.Contains("user@example.com", body);
        Assert.Contains("verify-token-123", body);
        Assert.Contains("verify-email", body);
        Assert.Contains("token=verify-token-123", body);
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastRequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}
