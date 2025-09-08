namespace SmartFlowBackend.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Category { get; }
    IRecordRepository Record { get; }
    ITagRepository Tag { get; }
    IUserRepository User { get; }
    IMonthlySummaryRepository MonthlySummary { get; }
    Task<int> SaveAsync();
}
