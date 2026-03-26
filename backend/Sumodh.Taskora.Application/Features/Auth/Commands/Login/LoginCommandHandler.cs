using Sumodh.Taskora.Application.Abstractions.Authentication;
using Sumodh.Taskora.Application.Abstractions.Persistence;
using Sumodh.Taskora.Application.Features.Auth.Dtos;
using Sumodh.Taskora.Application.Features.Auth.Exceptions;

namespace Sumodh.Taskora.Application.Features.Auth.Commands.Login
{
    public sealed class LoginCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJWTTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public LoginCommandHandler(IUserRepository userRepository,IPasswordHasher passwordHasher,IJWTTokenGenerator jwtTokenGenerator,IRefreshTokenGenerator refreshTokenGenerator,IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenGenerator = refreshTokenGenerator;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<AuthResponseDto> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var email = command.Email.Trim().ToLowerInvariant();

            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user is null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");
            if (!user.IsEmailVerified)
                throw new EmailNotVerifiedException();

            var accessToken = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, user.Name);

            var rawRefreshToken = _refreshTokenGenerator.Generate();
            var hashedRefreshToken = _refreshTokenGenerator.Hash(rawRefreshToken);

            var refreshToken = new Domain.Authentication.RefreshToken(
                user.Id,
                hashedRefreshToken,
                DateTime.UtcNow.AddDays(7));

            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }
    }
}
