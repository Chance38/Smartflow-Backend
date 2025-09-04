using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Domain.Interfaces;

public interface IRecordService
{
    Task AddRecordAsync(AddRecordRequest request, Guid userId);
    Task<List<Expense>> GetThisMonthExpensesAsync(Guid userId);
    Task<List<RecordPerMonth>> GetThisMonthRecordsAsync(Guid userId);
    Task<List<RecordPerMonth>> GetLastSixMonthRecordsAsync(Guid userId);
    Task<List<RecordPerMonth>> GetAllMonthRecordsAsync(Guid userId);
}
