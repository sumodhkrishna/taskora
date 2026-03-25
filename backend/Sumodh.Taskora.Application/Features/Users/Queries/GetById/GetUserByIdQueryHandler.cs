using Sumodh.Taskora.Api.Contracts.Users;
using Sumodh.Taskora.Application.Abstractions.Persistence;

namespace Sumodh.Taskora.Application.Features.Users.Queries.GetById
{
    public class GetUserByIdQueryHandler
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(query.Id, cancellationToken);

            if (user is null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }
    }
}
