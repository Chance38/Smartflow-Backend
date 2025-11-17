using Domain.Entity;

namespace Domain.Interface;

public interface IBalanceRepository : IRepository<Balance>
{
    Task<float> GetBalanceAsync(Guid userId);
    Task UpdateBalanceAsync(Guid userId, Entity.CategoryType type, float amount);
}
