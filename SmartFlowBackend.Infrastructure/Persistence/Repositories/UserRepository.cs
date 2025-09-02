using Microsoft.EntityFrameworkCore;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;

namespace SmartFlowBackend.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PostgresDbContext _context;

    public UserRepository(
        PostgresDbContext context
        )
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<float> GetInitialBalanceAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user.InitialBalance;
    }
}