using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;

namespace SmartFlowBackend.Domain.Services;

public class SummaryService : ISummaryService
{
    private readonly IUnitOfWork _unitOfWork;

    public SummaryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task UpdateMonthlySummaryAsync(User user, CategoryType type, int year, int month, float amount)
    {
        var summary = await _unitOfWork.MonthlySummary.FindAsync(s => s.UserId == user.UserId && s.Year == year && s.Month == month);
        if (summary == null)
        {
            summary = new MonthlySummary
            {
                MonthlySummaryId = Guid.NewGuid(),
                UserId = user.UserId,
                Year = year,
                Month = month,
                Income = 0,
                Expense = 0
            };

            switch (type)
            {
                case CategoryType.Expense:
                    summary.Expense = amount;
                    break;

                case CategoryType.Income:
                    summary.Income = amount;
                    break;

                default:
                    throw new ArgumentException(nameof(type), "Invalid category type");
            }
            await _unitOfWork.MonthlySummary.AddAsync(summary);
        }
        else
        {
            switch (type)
            {
                case CategoryType.Expense:
                    summary.Expense += amount;
                    break;

                case CategoryType.Income:
                    summary.Income += amount;
                    break;

                default:
                    throw new ArgumentException(nameof(type), "Invalid category type");
            }

            await _unitOfWork.MonthlySummary.UpdateAsync(summary);
        }
    }
}