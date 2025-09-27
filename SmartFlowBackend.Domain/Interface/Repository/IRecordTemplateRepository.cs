using Domain.Entity;

namespace Domain.Interface;

public interface IRecordTemplateRepository : IRepository<RecordTemplate>
{
    Task<RecordTemplate?> CheckExistAsync(Guid userId, string name);
    Task<List<RecordTemplate>> GetAllRecordTemplatesAsync(Guid userId);
    Task DeleteAsync(Guid userId, string name);
}
