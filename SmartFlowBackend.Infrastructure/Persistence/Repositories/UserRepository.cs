using Microsoft.EntityFrameworkCore;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

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

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> FindAsync(Expression<Func<User, bool>> expression, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null)
    {
        IQueryable<User> query = _context.Users;

        if (include != null)
        {
            query = include(query);
        }

        return await query.FirstOrDefaultAsync(expression);
    }

    public async Task<IEnumerable<User>> FindAllAsync(Expression<Func<User, bool>> expression, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null)
    {
        IQueryable<User> query = _context.Users;

        if (include != null)
        {
            query = include(query);
        }

        return await query.Where(expression).ToListAsync();
    }

    public async Task AddAsync(User entity)
    {
        await _context.Users.AddAsync(entity);
    }

    public Task UpdateAsync(User entity)
    {
        _context.Users.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
    }
}