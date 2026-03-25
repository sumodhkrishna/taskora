using Sumodh.Taskora.Application.Abstractions.Authentication;
using Sumodh.Taskora.Application.Abstractions.Persistence;

namespace Sumodh.Taskora.Application.Features.Auth.Commands.RequestPasswordReset
{
    public sealed class RequestPasswordResetCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenGenerator _tokenGenerator;

        public RequestPasswordResetCommandHandler(IUserRepository userRepository,IPasswordResetTokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<string?> Handle(RequestPasswordResetCommand command,CancellationToken cancellationToken)
        {
            var email = command.Email.Trim().ToLowerInvariant();

            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            if (user is null)
            {
                return null;
            }

            var rawToken = _tokenGenerator.Generate();
            var hashedToken = _tokenGenerator.Hash(rawToken);
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(30);

            user.SetPasswordResetToken(hashedToken, expiresAtUtc);

            await _userRepository.SaveChangesAsync(cancellationToken);

            return rawToken;
        }
    }
}
