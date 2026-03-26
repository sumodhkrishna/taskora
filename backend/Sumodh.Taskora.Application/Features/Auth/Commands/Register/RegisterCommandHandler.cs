using Sumodh.Taskora.Api.Contracts.Users;
using Sumodh.Taskora.Application.Abstractions.Authentication;
using Sumodh.Taskora.Application.Abstractions.Communication;
using Sumodh.Taskora.Application.Abstractions.Persistence;
using Sumodh.Taskora.Domain.Users;

namespace Sumodh.Taskora.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailVerificationTokenGenerator _emailVerificationTokenGenerator;
        private readonly IEmailVerificationEmailSender _emailVerificationEmailSender;

        public RegisterCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IEmailVerificationTokenGenerator emailVerificationTokenGenerator,
            IEmailVerificationEmailSender emailVerificationEmailSender)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _emailVerificationTokenGenerator = emailVerificationTokenGenerator;
            _emailVerificationEmailSender = emailVerificationEmailSender;
        }

        public async Task<UserDto> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            var normalizedEmail = command.Email.Trim().ToLowerInvariant();

            var emailExists = await _userRepository.EmailExistsAsync(normalizedEmail, cancellationToken);
            if (emailExists)
                throw new InvalidOperationException("A user with this email already exists.");

            var passwordHash = _passwordHasher.Hash(command.Password);
            var rawVerificationToken = _emailVerificationTokenGenerator.Generate();
            var hashedVerificationToken = _emailVerificationTokenGenerator.Hash(rawVerificationToken);

            var user = new User(command.Name, normalizedEmail, passwordHash);
            user.SetEmailVerificationToken(hashedVerificationToken, DateTime.UtcNow.AddHours(24));

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
            await _emailVerificationEmailSender.SendAsync(user.Name, user.Email, rawVerificationToken, cancellationToken);

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }
    }
}
