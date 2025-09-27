using Domain.Entity;
using Domain.Interface;

namespace Infrastructure.Persistence.Repository;

public class BalanceRepository : Repository<Balance>, IBalanceRepository
{
    public BalanceRepository(PostgresDbContext context) : base(context)
    {
    }

    public async Task<float> GetBalanceAsync(Guid userId)
    {
        var balance = await FindAsync(b => b.UserId == userId);
        if (balance == null)
        {
            throw new InvalidOperationException("Balance record not found for the user.");
        }

        return balance.Amount;
    }

    public async Task UpdateBalanceAsync(Guid userId, Domain.Entity.CategoryType type, float amount)
    {
        var balance = await FindAsync(b => b.UserId == userId);
        if (balance == null)
        {
            throw new InvalidOperationException("Balance record not found for the user.");
        }

        if (type == CategoryType.INCOME)
        {
            balance.Amount += amount;
        }
        else if (type == CategoryType.EXPENSE)
        {
            balance.Amount -= amount;
        }

        await UpdateAsync(balance);
    }
}