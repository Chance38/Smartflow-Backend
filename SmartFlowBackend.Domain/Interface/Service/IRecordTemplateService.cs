using Domain.Contract;

namespace Domain.Interface;

public interface IRecordTemplateService
{
    Task AddRecordTemplateAsync(Guid userId, RecordTemplate recordTemplate);
    Task<List<RecordTemplate>> GetAllRecordTemplatesAsync(Guid userId);
    Task DeleteRecordTemplateAsync(Guid userId, string name);
}
