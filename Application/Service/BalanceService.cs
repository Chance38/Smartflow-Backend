using Application.Contract;
using Application.Interface.Service;
using Domain.Interface;

namespace Application.Service;

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

    public async Task UpdateBalanceAsync(Guid userId, CategoryType type, float amount)
    {
        switch (type)
        {
            case CategoryType.EXPENSE:
                await _repo.UpdateBalanceAsync(userId, Domain.Entity.CategoryType.EXPENSE, amount);
                break;

            case CategoryType.INCOME:
                await _repo.UpdateBalanceAsync(userId, Domain.Entity.CategoryType.INCOME, amount);
                break;

            default:
                throw new ArgumentException("Invalid category type");
        }
    }
}
