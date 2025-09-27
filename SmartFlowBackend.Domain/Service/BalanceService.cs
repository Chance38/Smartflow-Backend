using Domain.Interface;

namespace Domain.Service;

public class BalanceService : IBalanceService
{
    private readonly IBalanceRepository _repo;

    public BalanceService(IBalanceRepository repo)
    {
        _repo = repo;
    }

    public async Task<float> GetBalanceAsync(Guid userId)
    {
        // InvalidOperationException will be thrown if balance record not found
        return await _repo.GetBalanceAsync(userId);
    }

    public async Task UpdateBalanceAsync(Guid userId, Contract.CategoryType type, float amount)
    {
        switch (type)
        {
            case Contract.CategoryType.EXPENSE:
                await _repo.UpdateBalanceAsync(userId, Entity.CategoryType.EXPENSE, amount);
                break;

            case Contract.CategoryType.INCOME:
                await _repo.UpdateBalanceAsync(userId, Entity.CategoryType.INCOME, amount);
                break;

            default:
                throw new ArgumentException("Invalid category type");
        }
    }
}
