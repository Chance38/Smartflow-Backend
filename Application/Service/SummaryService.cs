using Application.Contract;
using Application.Interface.Service;
using Domain.Interface;

namespace Application.Service;

public class SummaryService : ISummaryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISummaryRepository _repo;

    public SummaryService(IUnitOfWork unitOfWork, ISummaryRepository repo)
    {
        _unitOfWork = unitOfWork;
        _repo = repo;
    }

    public async Task<List<SummaryPerMonth>> GetSummariesAsync(Guid userId, int startYear, int startMonth, int endYear, int endMonth)
    {
        var summaries = await _repo.GetAllSummariesAsync(userId, startYear, startMonth, endYear, endMonth);
        var dict = summaries.ToDictionary(s => (s.Year, s.Month));

        int total = (endYear - startYear) * 12 + endMonth - startMonth + 1;
        var records = new List<SummaryPerMonth>(total);

        for (int i = 0; i < total; i++)
        {
            int year = startYear + (startMonth + i - 1) / 12;
            int month = (startMonth + i - 1) % 12 + 1;

            if (dict.TryGetValue((year, month), out var summary))
            {
                records.Add(new SummaryPerMonth
                {
                    Year = year,
                    Month = month,
                    Expense = summary.Expense,
                    Income = summary.Income
                });
            }
            else
            {
                records.Add(new SummaryPerMonth
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

    public async Task<List<SummaryPerMonth>> GetAllSummariesAsync(Guid userId)
    {
        var summaries = await _repo.GetAllSummariesAsync(userId);
        return summaries
            .OrderBy(s => s.Year).ThenBy(s => s.Month)
            .Select(s => new SummaryPerMonth
            {
                Year = s.Year,
                Month = s.Month,
                Expense = s.Expense,
                Income = s.Income
            }).ToList();
    }

    public async Task UpdateMonthlySummaryAsync(Guid userId, CategoryType type, int year, int month, float amount)
    {
        var typeEntity = type switch
        {
            CategoryType.EXPENSE => Domain.Entity.CategoryType.EXPENSE,
            CategoryType.INCOME => Domain.Entity.CategoryType.INCOME,
            _ => throw new ArgumentException("Invalid category type")
        };

        await _repo.UpdateAsync(userId, typeEntity, year, month, amount);
    }
}