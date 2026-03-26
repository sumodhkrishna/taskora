using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Sumodh.Taskora.Application.Abstractions.Communication;

namespace Sumodh.Taskora.Infra.Email
{
    public sealed class SendGridPasswordResetEmailSender : IPasswordResetEmailSender
    {
        private readonly HttpClient _httpClient;
        private readonly SendGridEmailOptions _options;

        public SendGridPasswordResetEmailSender(HttpClient httpClient,IOptions<SendGridEmailOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task SendAsync(string name, string email, string resetToken, CancellationToken cancellationToken)
        {
            var resetUrl = QueryHelpers.AddQueryString(_options.PasswordResetUrl, new Dictionary<string, string?>
            {
                ["email"] = email,
                ["token"] = resetToken
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
                        subject = "Reset your Taskora password"
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
                            $"Use the link below to reset your Taskora password:{Environment.NewLine}{resetUrl}{Environment.NewLine}{Environment.NewLine}" +
                            $"If you prefer, you can also paste this reset token manually: {resetToken}{Environment.NewLine}{Environment.NewLine}" +
                            "If you did not request a password reset, you can ignore this email."
                    },
                    new
                    {
                        type = "text/html",
                        value =
                            $"<p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p>" +
                            $"<p>Use the URL below to reset your Taskora password:</p>" +
                            $"<p>{System.Net.WebUtility.HtmlEncode(resetUrl)}</p>" +
                            $"<p>If you prefer, you can also paste this reset token manually:</p>" +
                            $"<p><strong>{System.Net.WebUtility.HtmlEncode(resetToken)}</strong></p>" +
                            "<p>If you did not request a password reset, you can ignore this email.</p>"
                    }
                }
            });

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
