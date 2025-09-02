using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Domain.Interfaces;

public interface IRecordRepository : IRepository<Record>
{
    Task<IEnumerable<Record>> GetRecordsByUserIdAndMonthAsync(Guid userId, int year, int month);
    Task<IEnumerable<Record>> GetRecordsByUserIdAsync(Guid userId);
    Task<List<MonthlyRecordView>> GetMonthlyRecordsViewAsync(Guid userId);
    Task<BalanceView?> GetBalanceViewAsync(Guid userId);
}
