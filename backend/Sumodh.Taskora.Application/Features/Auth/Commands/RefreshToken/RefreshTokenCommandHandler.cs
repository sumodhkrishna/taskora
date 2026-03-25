using Sumodh.Taskora.Application.Abstractions.Authentication;
using Sumodh.Taskora.Application.Abstractions.Persistence;
using Sumodh.Taskora.Application.Features.Auth.Dtos;
using Sumodh.Taskora.Domain.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Application.Features.Auth.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandHandler
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJWTTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;

        public RefreshTokenCommandHandler(IRefreshTokenRepository refreshTokenRepository,IUserRepository userRepository,IJWTTokenGenerator jwtTokenGenerator,IRefreshTokenGenerator refreshTokenGenerator)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenGenerator = refreshTokenGenerator;
        }

        public async Task<AuthResponseDto?> Handle(
            RefreshTokenCommand command,
            CancellationToken cancellationToken)
        {
            var incomingHash = _refreshTokenGenerator.Hash(command.RefreshToken);

            var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(incomingHash, cancellationToken);
            if (existingToken is null || !existingToken.IsActive(DateTime.UtcNow))
                return null;

            var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
            if (user is null)
                return null;

            var newRawRefreshToken = _refreshTokenGenerator.Generate();
            var newHashedRefreshToken = _refreshTokenGenerator.Hash(newRawRefreshToken);

            existingToken.Revoke(newHashedRefreshToken);

            var replacementToken = new Domain.Authentication.RefreshToken(
                user.Id,
                newHashedRefreshToken,
                DateTime.UtcNow.AddDays(7));

            await _refreshTokenRepository.AddAsync(replacementToken, cancellationToken);
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            var newAccessToken = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, user.Name);

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRawRefreshToken,
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }
    }
}
