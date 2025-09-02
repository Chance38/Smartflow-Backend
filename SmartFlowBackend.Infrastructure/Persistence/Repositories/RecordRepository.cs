using Microsoft.EntityFrameworkCore;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;

namespace SmartFlowBackend.Infrastructure.Persistence.Repositories;

public class RecordRepository : Repository<Record>, IRecordRepository
{
    public RecordRepository(PostgresDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Record>> GetRecordsByUserIdAndMonthAsync(Guid userId, int year, int month)
    {
        return await _context.Records
            .Include(r => r.Category)
            .Where(r => r.Category.UserId == userId && r.Date.Year == year && r.Date.Month == month)
            .ToListAsync();
    }

    public async Task<IEnumerable<Record>> GetRecordsByUserIdAsync(Guid userId)
    {
        return await _context.Records
            .Include(r => r.Category)
            .Where(r => r.Category.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<MonthlyRecordView>> GetMonthlyRecordsViewAsync(Guid userId)
    {
        return await _context.Set<MonthlyRecordView>()
            .FromSqlRaw("SELECT * FROM monthly_records_view WHERE \"UserId\" = {0}", userId)
            .ToListAsync();
    }

    public async Task<BalanceView?> GetBalanceViewAsync(Guid userId)
    {
        return await _context.Set<BalanceView>()
            .FromSqlRaw("SELECT * FROM balance_view WHERE \"UserId\" = {0}", userId)
            .FirstOrDefaultAsync();
    }
}