using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;

namespace SmartFlowBackend.Infrastructure.Persistence.Repositories;

public class RecordRepository : Repository<Record>, IRecordRepository
{
    public RecordRepository(PostgresDbContext context) : base(context)
    {
    }
}