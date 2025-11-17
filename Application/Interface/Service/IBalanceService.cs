using Application.Contract;

namespace Application.Interface.Service;

public interface IBalanceService
{
    Task<float> GetBalanceAsync(Guid userId);
    Task UpdateBalanceAsync(Guid userId, CategoryType type, float amount);
}
