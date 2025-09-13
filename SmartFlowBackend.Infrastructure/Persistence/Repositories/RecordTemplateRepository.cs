using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;

namespace SmartFlowBackend.Infrastructure.Persistence.Repositories;

public class RecordTemplateRepository : Repository<RecordTemplate>, IRecordTemplateRepository
{
    public RecordTemplateRepository(PostgresDbContext context) : base(context)
    {
    }
}