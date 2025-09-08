using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Domain.Interfaces
{
    public interface ISummaryService
    {
        Task UpdateMonthlySummaryAsync(User user, CategoryType type, int year, int month, float amount);
    }
}
