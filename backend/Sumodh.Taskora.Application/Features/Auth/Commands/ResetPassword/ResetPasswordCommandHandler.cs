using Sumodh.Taskora.Application.Abstractions.Authentication;
using Sumodh.Taskora.Application.Abstractions.Persistence;

namespace Sumodh.Taskora.Application.Features.Auth.Commands.ResetPassword
{
    public sealed class ResetPasswordCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenGenerator _tokenGenerator;
        private readonly IPasswordHasher _passwordHasher;

        public ResetPasswordCommandHandler(IUserRepository userRepository,IPasswordResetTokenGenerator tokenGenerator,IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _tokenGenerator = tokenGenerator;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> Handle(ResetPasswordCommand command,CancellationToken cancellationToken)
        {
            var email = command.Email.Trim().ToLowerInvariant();

            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            if (user is null)
                return false;

            var tokenHash = _tokenGenerator.Hash(command.Token.Trim());

            if (!user.CanUsePasswordResetToken(tokenHash, DateTime.UtcNow))
                return false;

            var newPasswordHash = _passwordHasher.Hash(command.NewPassword);

            user.ResetPassword(newPasswordHash);

            await _userRepository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
