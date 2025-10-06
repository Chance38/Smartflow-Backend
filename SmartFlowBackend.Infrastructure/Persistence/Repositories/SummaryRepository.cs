using Domain.Entity;
using Domain.Interface;

namespace Infrastructure.Persistence.Repository;

public class SummaryRepository : Repository<MonthlySummary>, ISummaryRepository
{
    public SummaryRepository(PostgresDbContext context) : base(context)
    {
    }

    public async Task<List<MonthlySummary>> GetAllSummariesAsync(Guid userId, int startYear, int startMonth, int endYear, int endMonth)
    {
        var summaries = await FindAllAsync(s => s.UserId == userId &&
            (s.Year > startYear || (s.Year == startYear && s.Month >= startMonth)) &&
            (s.Year < endYear || (s.Year == endYear && s.Month <= endMonth)));

        return summaries.ToList();
    }

    public async Task<List<MonthlySummary>> GetAllSummariesAsync(Guid userId)
    {
        var summaries = await FindAllAsync(s => s.UserId == userId);

        if (summaries == null || !summaries.Any())
        {
            return new List<MonthlySummary>();
        }

        return summaries.ToList();
    }

    public async Task UpdateAsync(Guid userId, CategoryType type, int year, int month, float amount)
    {
        var summary = await FindAsync(s => s.UserId == userId && s.Year == year && s.Month == month);
        if (summary == null)
        {
            summary = new MonthlySummary
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Year = year,
                Month = month,
                Income = 0,
                Expense = 0,
            };

            if (type == CategoryType.INCOME)
            {
                summary.Income = amount;
            }
            else if (type == CategoryType.EXPENSE)
            {
                summary.Expense = amount;
            }

            await AddAsync(summary);
        }
        else
        {
            if (type == CategoryType.INCOME)
            {
                summary.Income += amount;
            }
            else if (type == CategoryType.EXPENSE)
            {
                summary.Expense += amount;
            }

            await UpdateAsync(summary);
        }
    }
}
