using SmartFlowBackend.Domain.Interfaces;
using SmartFlowBackend.Infrastructure.Persistence.Repositories;

namespace SmartFlowBackend.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly PostgresDbContext _context;
    public ICategoryRepository Category { get; }
    public IRecordRepository Record { get; }
    public ITagRepository Tag { get; }
    public IUserRepository User { get; }
    public IMonthlySummaryRepository MonthlySummary { get; }

    public UnitOfWork(PostgresDbContext context)
    {
        _context = context;
        Category = new CategoryRepository(_context);
        Record = new RecordRepository(_context);
        Tag = new TagRepository(_context);
        User = new UserRepository(_context);
        MonthlySummary = new MonthlySummaryRepository(_context);
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
