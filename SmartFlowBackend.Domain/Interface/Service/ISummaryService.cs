using Domain.Contract;

namespace Domain.Interface
{
    public interface ISummaryService
    {
        Task<List<SummaryPerMonth>> GetSummariesAsync(Guid userId, int startYear, int startMonth, int endYear, int endMonth);
        Task<List<SummaryPerMonth>> GetAllSummariesAsync(Guid userId);
        Task UpdateMonthlySummaryAsync(Guid userId, CategoryType type, int year, int month, float amount);
    }
}
