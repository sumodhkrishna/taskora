using Sumodh.Taskora.Application.Abstractions.Authentication;
using Sumodh.Taskora.Application.Abstractions.Communication;
using Sumodh.Taskora.Application.Abstractions.Persistence;

namespace Sumodh.Taskora.Application.Features.Auth.Commands.RequestPasswordReset
{
    public sealed class RequestPasswordResetCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenGenerator _tokenGenerator;
        private readonly IPasswordResetEmailSender _passwordResetEmailSender;

        public RequestPasswordResetCommandHandler(
            IUserRepository userRepository,
            IPasswordResetTokenGenerator tokenGenerator,
            IPasswordResetEmailSender passwordResetEmailSender)
        {
            _userRepository = userRepository;
            _tokenGenerator = tokenGenerator;
            _passwordResetEmailSender = passwordResetEmailSender;
        }

        public async Task<bool> Handle(RequestPasswordResetCommand command,CancellationToken cancellationToken)
        {
            var email = command.Email.Trim().ToLowerInvariant();

            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user is null)
            {
                return false;
            }

            var rawToken = _tokenGenerator.Generate();
            var hashedToken = _tokenGenerator.Hash(rawToken);
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(30);

            user.SetPasswordResetToken(hashedToken, expiresAtUtc);

            await _userRepository.SaveChangesAsync(cancellationToken);
            await _passwordResetEmailSender.SendAsync(user.Name, user.Email, rawToken, cancellationToken);
            return true;
        }
    }
}
