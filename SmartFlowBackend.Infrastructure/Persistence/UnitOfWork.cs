using Domain.Interface;
using Infrastructure.Persistence.Repository;

namespace Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly PostgresDbContext _context;
    public IBalanceRepository Balance { get; }
    public ICategoryRepository Category { get; }
    public IRecordRepository Record { get; }
    public ITagRepository Tag { get; }
    public IMonthlySummaryRepository MonthlySummary { get; }
    public IRecordTemplateRepository RecordTemplate { get; }

    public UnitOfWork(PostgresDbContext context)
    {
        _context = context;
        Balance = new BalanceRepository(_context);
        Category = new CategoryRepository(_context);
        Record = new RecordRepository(_context);
        Tag = new TagRepository(_context);
        MonthlySummary = new MonthlySummaryRepository(_context);
        RecordTemplate = new RecordTemplateRepository(_context);
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
