using Microsoft.EntityFrameworkCore;
using Sumodh.Taskora.Application.Abstractions.Persistence;
using Sumodh.Taskora.Domain.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sumodh.Taskora.Infra.Persistance.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext) => _dbContext = dbContext;

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) => await _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);

        public async Task AddAsync(User user, CancellationToken cancellationToken) => await _dbContext.Users.AddAsync(user, cancellationToken);

        public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken) => await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) => await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        public async Task SaveChangesAsync(CancellationToken cancellationToken) => await _dbContext.SaveChangesAsync(cancellationToken);
        
    }
}
