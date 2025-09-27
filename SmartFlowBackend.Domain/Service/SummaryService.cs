using Domain.Interface;

namespace Domain.Service;

public class SummaryService : ISummaryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMonthlySummaryRepository _repo;

    public SummaryService(IUnitOfWork unitOfWork, IMonthlySummaryRepository repo)
    {
        _unitOfWork = unitOfWork;
        _repo = repo;
    }

    public async Task<List<Contract.RecordPerMonth>> GetSummariesAsync(Guid userId, int startYear, int startMonth, int endYear, int endMonth)
    {
        var summaries = await _repo.GetAllSummariesAsync(userId, startYear, startMonth, endYear, endMonth);
        var dict = summaries.ToDictionary(s => (s.Year, s.Month));

        int total = (endYear - startYear) * 12 + endMonth - startMonth + 1;
        var records = new List<Contract.RecordPerMonth>(total);

        for (int i = 0; i < total; i++)
        {
            int year = startYear + (startMonth + i - 1) / 12;
            int month = (startMonth + i - 1) % 12 + 1;

            if (dict.TryGetValue((year, month), out var summary))
            {
                records.Add(new Contract.RecordPerMonth
                {
                    Year = year,
                    Month = month,
                    Expense = summary.Expense,
                    Income = summary.Income
                });
            }
            else
            {
                records.Add(new Contract.RecordPerMonth
                {
                    Year = year,
                    Month = month,
                    Expense = 0,
                    Income = 0
                });
            }
        }

        return records;
    }

    public async Task<List<Contract.RecordPerMonth>> GetAllSummariesAsync(Guid userId)
    {
        var summaries = await _repo.GetAllSummariesAsync(userId);
        return summaries
            .OrderBy(s => s.Year).ThenBy(s => s.Month)
            .Select(s => new Contract.RecordPerMonth
            {
                Year = s.Year,
                Month = s.Month,
                Expense = s.Expense,
                Income = s.Income
            }).ToList();
    }

    public async Task UpdateMonthlySummaryAsync(Guid userId, Contract.CategoryType type, int year, int month, float amount)
    {
        var typeEntity = type switch
        {
            Contract.CategoryType.EXPENSE => Entity.CategoryType.EXPENSE,
            Contract.CategoryType.INCOME => Entity.CategoryType.INCOME,
            _ => throw new ArgumentException("Invalid category type")
        };

        await _repo.UpdateAsync(userId, typeEntity, year, month, amount);
    }
}