using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;
using SmartFlowBackend.Infrastructure.Persistence;

namespace SmartFlowBackend.Infrastructure.Persistence.Repositories;

public class MonthlySummaryRepository : Repository<MonthlySummary>, IMonthlySummaryRepository
{
    public MonthlySummaryRepository(PostgresDbContext context) : base(context)
    {
    }
}
