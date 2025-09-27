using Domain.Contract;

namespace Domain.Interface;

public interface IRecordService
{
    Task AddRecordAsync(Guid userId, AddRecordRequest req);
    Task<List<Expense>> GetThisMonthExpensesAsync(Guid userId);
}
