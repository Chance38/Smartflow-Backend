using Application.Contract;

namespace Application.Interface.Service;

public interface IRecordService
{
    Task AddRecordAsync(Guid userId, AddRecordRequest req);
    Task<List<Expense>> GetThisMonthExpensesAsync(Guid userId);
}
