using Domain.Entity;
using Domain.Interface;

namespace Infrastructure.Persistence.Repository;

public class RecordRepository : Repository<Record>, IRecordRepository
{
    public RecordRepository(PostgresDbContext context) : base(context)
    {
    }

    public async Task<List<Record>> GetAllRecordsAsync(Guid userId, int year, int month)
    {
        var records = await FindAllAsync(r => r.UserId == userId && r.Date.Year == year && r.Date.Month == month);
        return records.ToList();
    }
}