using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Domain.Interfaces;

public interface IBalanceService
{
    Task<float> GetBalanceByUserIdAsync(Guid userId);
    Task UpdateBalanceAsync(User user, CategoryType type, float amount);
}
