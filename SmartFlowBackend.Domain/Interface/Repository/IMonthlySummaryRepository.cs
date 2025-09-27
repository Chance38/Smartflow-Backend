using Domain.Entity;

namespace Domain.Interface;

public interface IMonthlySummaryRepository : IRepository<MonthlySummary>
{
    public Task<List<MonthlySummary>> GetAllSummariesAsync(Guid userId, int startYear, int startMonth, int endYear, int endMonth);
    public Task<List<MonthlySummary>> GetAllSummariesAsync(Guid userId);
    public Task UpdateAsync(Guid userId, CategoryType type, int year, int month, float amount);
}
