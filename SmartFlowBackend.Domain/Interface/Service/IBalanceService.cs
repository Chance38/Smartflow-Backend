using Domain.Contract;

namespace Domain.Interface;

public interface IBalanceService
{
    Task<float> GetBalanceAsync(Guid userId);
    Task UpdateBalanceAsync(Guid userId, CategoryType type, float amount);
}
