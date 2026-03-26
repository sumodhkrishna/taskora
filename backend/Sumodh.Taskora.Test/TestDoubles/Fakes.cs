using Sumodh.Taskora.Application.Abstractions.Authentication;
using Sumodh.Taskora.Application.Abstractions.Communication;
using Sumodh.Taskora.Application.Abstractions.Identity;
using Sumodh.Taskora.Application.Abstractions.Persistence;
using Sumodh.Taskora.Domain.Authentication;
using Sumodh.Taskora.Domain.Todos;
using Sumodh.Taskora.Domain.Users;

namespace Sumodh.Taskora.Test.TestDoubles;

internal sealed class FakeUserRepository : IUserRepository
{
    public bool EmailExistsResult { get; set; }
    public User? UserByIdResult { get; set; }
    public User? UserByEmailResult { get; set; }
    public string? LastEmailExistsArgument { get; private set; }
    public string? LastGetByEmailArgument { get; private set; }
    public User? AddedUser { get; private set; }
    public int SaveChangesCallCount { get; private set; }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        LastEmailExistsArgument = email;
        return Task.FromResult(EmailExistsResult);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        AddedUser = user;
        return Task.CompletedTask;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return Task.FromResult(UserByIdResult);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        LastGetByEmailArgument = email;
        return Task.FromResult(UserByEmailResult);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}

internal sealed class FakeTodoRepository : ITodoRepository
{
    public TodoItem? TodoByIdResult { get; set; }
    public TodoItem? TodoByIdForUserResult { get; set; }
    public List<TodoItem> TodosByUserIdResult { get; set; } = [];
    public List<TodoItem> PagedTodosResult { get; set; } = [];
    public int PagedTotalCountResult { get; set; }
    public TodoItem? AddedTodo { get; private set; }
    public TodoItem? RemovedTodo { get; private set; }
    public int SaveChangesCallCount { get; private set; }
    public int? LastGetByUserIdArgument { get; private set; }
    public (int Id, int UserId)? LastGetByIdForUserArguments { get; private set; }
    public (int UserId, bool? IsCompleted, int? Priority, string? Search, DateTime? DueBefore, int Page, int PageSize)? LastGetPagedForUserArguments { get; private set; }

    public Task AddAsync(TodoItem todoItem, CancellationToken cancellationToken)
    {
        AddedTodo = todoItem;
        return Task.CompletedTask;
    }

    public Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return Task.FromResult(TodoByIdResult);
    }

    public Task<List<TodoItem>> GetByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        LastGetByUserIdArgument = userId;
        return Task.FromResult(TodosByUserIdResult);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }

    public Task<TodoItem?> GetByIdForUserAsync(int id, int userId, CancellationToken cancellationToken)
    {
        LastGetByIdForUserArguments = (id, userId);
        return Task.FromResult(TodoByIdForUserResult);
    }

    public void Remove(TodoItem todoItem)
    {
        RemovedTodo = todoItem;
    }

    public Task<(List<TodoItem> Items, int TotalCount)> GetPagedForUserAsync(int userId, bool? isCompleted, int? priority, string? search, DateTime? dueBefore, int page, int pageSize, CancellationToken cancellationToken)
    {
        LastGetPagedForUserArguments = (userId, isCompleted, priority, search, dueBefore, page, pageSize);
        return Task.FromResult((PagedTodosResult, PagedTotalCountResult));
    }
}

internal sealed class FakeCurrentUserService : ICurrentUserService
{
    public int UserId { get; init; }
    public string? Email { get; init; }
    public string? Name { get; init; }
}

internal sealed class StubPasswordHasher : IPasswordHasher
{
    public string HashResult { get; set; } = "hashed-password";
    public bool VerifyResult { get; set; }
    public string? LastHashArgument { get; private set; }
    public (string Password, string PasswordHash)? LastVerifyArguments { get; private set; }

    public string Hash(string password)
    {
        LastHashArgument = password;
        return HashResult;
    }

    public bool Verify(string password, string passwordHash)
    {
        LastVerifyArguments = (password, passwordHash);
        return VerifyResult;
    }
}

internal sealed class StubJwtTokenGenerator : IJWTTokenGenerator
{
    public string TokenToReturn { get; set; } = "generated-token";
    public (int UserId, string Email, string Name)? LastGenerateArguments { get; private set; }

    public string GenerateToken(int userId, string email, string name)
    {
        LastGenerateArguments = (userId, email, name);
        return TokenToReturn;
    }
}

internal sealed class StubRefreshTokenGenerator : IRefreshTokenGenerator
{
    public string GenerateResult { get; set; } = "raw-refresh-token";
    public string HashResult { get; set; } = "hashed-refresh-token";
    public string? LastHashArgument { get; private set; }
    public List<string> HashArguments { get; } = [];
    public int GenerateCallCount { get; private set; }

    public string Generate()
    {
        GenerateCallCount++;
        return GenerateResult;
    }

    public string Hash(string token)
    {
        LastHashArgument = token;
        HashArguments.Add(token);
        return HashResult;
    }
}

internal sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
{
    public RefreshToken? AddedRefreshToken { get; private set; }
    public RefreshToken? RefreshTokenByHashResult { get; set; }
    public List<RefreshToken> ActiveTokensByUserIdResult { get; set; } = [];
    public string? LastGetByTokenHashArgument { get; private set; }
    public int? LastGetActiveByUserIdArgument { get; private set; }
    public int SaveChangesCallCount { get; private set; }

    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        AddedRefreshToken = refreshToken;
        return Task.CompletedTask;
    }

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        LastGetByTokenHashArgument = tokenHash;
        return Task.FromResult(RefreshTokenByHashResult);
    }

    public Task<List<RefreshToken>> GetActiveByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        LastGetActiveByUserIdArgument = userId;
        return Task.FromResult(ActiveTokensByUserIdResult);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}

internal sealed class StubPasswordResetTokenGenerator : IPasswordResetTokenGenerator
{
    public string GenerateResult { get; set; } = "raw-reset-token";
    public string HashResult { get; set; } = "hashed-reset-token";
    public string? LastHashArgument { get; private set; }
    public int GenerateCallCount { get; private set; }

    public string Generate()
    {
        GenerateCallCount++;
        return GenerateResult;
    }

    public string Hash(string token)
    {
        LastHashArgument = token;
        return HashResult;
    }
}

internal sealed class StubPasswordResetEmailSender : IPasswordResetEmailSender
{
    public (string Name, string Email, string ResetToken)? LastSendArguments { get; private set; }
    public int SendCallCount { get; private set; }

    public Task SendAsync(string name, string email, string resetToken, CancellationToken cancellationToken)
    {
        LastSendArguments = (name, email, resetToken);
        SendCallCount++;
        return Task.CompletedTask;
    }
}

internal sealed class StubEmailVerificationTokenGenerator : IEmailVerificationTokenGenerator
{
    public string GenerateResult { get; set; } = "raw-verification-token";
    public string HashResult { get; set; } = "hashed-verification-token";
    public string? LastHashArgument { get; private set; }
    public int GenerateCallCount { get; private set; }

    public string Generate()
    {
        GenerateCallCount++;
        return GenerateResult;
    }

    public string Hash(string token)
    {
        LastHashArgument = token;
        return HashResult;
    }
}

internal sealed class StubEmailVerificationEmailSender : IEmailVerificationEmailSender
{
    public (string Name, string Email, string VerificationToken)? LastSendArguments { get; private set; }
    public int SendCallCount { get; private set; }

    public Task SendAsync(string name, string email, string verificationToken, CancellationToken cancellationToken)
    {
        LastSendArguments = (name, email, verificationToken);
        SendCallCount++;
        return Task.CompletedTask;
    }
}
