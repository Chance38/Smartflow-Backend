using Domain.Entity;

namespace Domain.Interface;

public interface IRecordRepository : IRepository<Record>
{
    Task<List<Record>> GetAllRecordsAsync(Guid userId, int year, int month);
}
