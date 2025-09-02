using SmartFlowBackend.Domain.Interfaces;
using SmartFlowBackend.Infrastructure.Persistence.Repositories;

namespace SmartFlowBackend.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly PostgresDbContext _context;
    public ICategoryRepository Categories { get; }
    public IRecordRepository Records { get; }
    public ITagRepository Tags { get; }
    public IUserRepository Users { get; }

    public UnitOfWork(PostgresDbContext context)
    {
        _context = context;
        Categories = new CategoryRepository(_context);
        Records = new RecordRepository(_context);
        Tags = new TagRepository(_context);
        Users = new UserRepository(_context);
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
