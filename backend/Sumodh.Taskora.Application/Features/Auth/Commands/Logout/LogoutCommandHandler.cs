using Sumodh.Taskora.Application.Abstractions.Authentication;
using Sumodh.Taskora.Application.Abstractions.Persistence;

namespace Sumodh.Taskora.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommandHandler
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;

        public LogoutCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IRefreshTokenGenerator refreshTokenGenerator)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenGenerator = refreshTokenGenerator;
        }

        public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
        {
            var tokenHash = _refreshTokenGenerator.Hash(command.RefreshToken);

            var refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
            if (refreshToken is null)
                return;

            refreshToken.Revoke();
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
