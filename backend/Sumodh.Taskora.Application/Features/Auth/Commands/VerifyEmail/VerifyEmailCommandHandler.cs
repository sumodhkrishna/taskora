using Sumodh.Taskora.Application.Abstractions.Authentication;
using Sumodh.Taskora.Application.Abstractions.Persistence;

namespace Sumodh.Taskora.Application.Features.Auth.Commands.VerifyEmail
{
    public sealed class VerifyEmailCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailVerificationTokenGenerator _tokenGenerator;

        public VerifyEmailCommandHandler(IUserRepository userRepository, IEmailVerificationTokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<bool> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
        {
            var email = command.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user is null)
            {
                return false;
            }

            var tokenHash = _tokenGenerator.Hash(command.Token.Trim());
            if (!user.CanUseEmailVerificationToken(tokenHash, DateTime.UtcNow))
            {
                return false;
            }

            user.MarkEmailVerified(DateTime.UtcNow);
            await _userRepository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
