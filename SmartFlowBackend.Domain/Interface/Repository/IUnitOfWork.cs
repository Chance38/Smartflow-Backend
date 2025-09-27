namespace Domain.Interface;

public interface IUnitOfWork : IDisposable
{
    IBalanceRepository Balance { get; }
    ICategoryRepository Category { get; }
    IRecordRepository Record { get; }
    ITagRepository Tag { get; }
    IMonthlySummaryRepository MonthlySummary { get; }
    IRecordTemplateRepository RecordTemplate { get; }
    Task<int> SaveAsync();
}
