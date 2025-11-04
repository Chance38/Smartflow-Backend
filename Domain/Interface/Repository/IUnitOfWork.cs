namespace Domain.Interface;

public interface IUnitOfWork : IDisposable
{
    IBalanceRepository Balance { get; }
    ICategoryRepository Category { get; }
    IRecordRepository Record { get; }
    ITagRepository Tag { get; }
    ISummaryRepository MonthlySummary { get; }
    IRecordTemplateRepository RecordTemplate { get; }
    Task<int> SaveAsync();
}
