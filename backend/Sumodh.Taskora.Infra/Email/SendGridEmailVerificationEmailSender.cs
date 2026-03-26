using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Sumodh.Taskora.Application.Abstractions.Communication;

namespace Sumodh.Taskora.Infra.Email
{
    public sealed class SendGridEmailVerificationEmailSender : IEmailVerificationEmailSender
    {
        private readonly HttpClient _httpClient;
        private readonly SendGridEmailOptions _options;

        public SendGridEmailVerificationEmailSender(HttpClient httpClient, IOptions<SendGridEmailOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task SendAsync(string name, string email, string verificationToken, CancellationToken cancellationToken)
        {
            var actionUrl = QueryHelpers.AddQueryString(_options.EmailVerificationUrl, new Dictionary<string, string?>
            {
                ["email"] = email,
                ["token"] = verificationToken
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, "mail/send");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            request.Content = JsonContent.Create(new
            {
                personalizations = new[]
                {
                    new
                    {
                        to = new[]
                        {
                            new { email, name }
                        },
                        subject = "Verify your Taskora email"
                    }
                },
                from = new
                {
                    email = _options.FromEmail,
                    name = _options.FromName
                },
                content = new object[]
                {
                    new
                    {
                        type = "text/plain",
                        value =
                            $"Hi {name},{Environment.NewLine}{Environment.NewLine}" +
                            $"Use the link below to verify your Taskora email:{Environment.NewLine}{actionUrl}{Environment.NewLine}{Environment.NewLine}" +
                            $"If you prefer, you can also paste this verification token manually: {verificationToken}{Environment.NewLine}{Environment.NewLine}" +
                            "If you did not create a Taskora account, you can ignore this email."
                    },
                    new
                    {
                        type = "text/html",
                        value =
                            $"<p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p>" +
                            $"<p>Use the link below to verify your Taskora email:</p>" +
                            $"<p><a href=\"{System.Net.WebUtility.HtmlEncode(actionUrl)}\">Verify your email</a></p>" +
                            $"<p>If you prefer, you can also paste this verification token manually:</p>" +
                            $"<p><strong>{System.Net.WebUtility.HtmlEncode(verificationToken)}</strong></p>" +
                            "<p>If you did not create a Taskora account, you can ignore this email.</p>"
                    }
                }
            });

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
