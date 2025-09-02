namespace SmartFlowBackend.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Categories { get; }
    IRecordRepository Records { get; }
    ITagRepository Tags { get; }
    IUserRepository Users { get; }
    Task<int> SaveAsync();
}
