using Microsoft.EntityFrameworkCore;
using Domain.Interface;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Infrastructure.Persistence.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly PostgresDbContext _context;

    public Repository(PostgresDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<T?> FindAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
    {
        IQueryable<T> query = _context.Set<T>();

        if (include != null)
        {
            query = include(query);
        }

        return await query.FirstOrDefaultAsync(expression);
    }

    public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
    {
        IQueryable<T> query = _context.Set<T>();

        if (include != null)
        {
            query = include(query);
        }

        return await query.Where(expression).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _context.Set<T>().AddRangeAsync(entities);
    }

    public Task UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.Set<T>().Remove(entity);
        }
    }
}
