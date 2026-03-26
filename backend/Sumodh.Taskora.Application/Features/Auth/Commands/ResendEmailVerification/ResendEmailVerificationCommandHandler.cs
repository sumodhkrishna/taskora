using Sumodh.Taskora.Application.Abstractions.Authentication;
using Sumodh.Taskora.Application.Abstractions.Communication;
using Sumodh.Taskora.Application.Abstractions.Persistence;

namespace Sumodh.Taskora.Application.Features.Auth.Commands.ResendEmailVerification
{
    public sealed class ResendEmailVerificationCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailVerificationTokenGenerator _tokenGenerator;
        private readonly IEmailVerificationEmailSender _emailSender;

        public ResendEmailVerificationCommandHandler(
            IUserRepository userRepository,
            IEmailVerificationTokenGenerator tokenGenerator,
            IEmailVerificationEmailSender emailSender)
        {
            _userRepository = userRepository;
            _tokenGenerator = tokenGenerator;
            _emailSender = emailSender;
        }

        public async Task<bool> Handle(ResendEmailVerificationCommand command, CancellationToken cancellationToken)
        {
            var email = command.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user is null || user.IsEmailVerified)
            {
                return false;
            }

            var rawToken = _tokenGenerator.Generate();
            var hashedToken = _tokenGenerator.Hash(rawToken);
            user.SetEmailVerificationToken(hashedToken, DateTime.UtcNow.AddHours(24));

            await _userRepository.SaveChangesAsync(cancellationToken);
            await _emailSender.SendAsync(user.Name, user.Email, rawToken, cancellationToken);
            return true;
        }
    }
}
