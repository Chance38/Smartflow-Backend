using Application.Contract;

namespace Application.Interface.Service;

public interface IRecordTemplateService
{
    Task AddRecordTemplateAsync(Guid userId, RecordTemplate recordTemplate);
    Task<List<RecordTemplate>> GetAllRecordTemplatesAsync(Guid userId);
    Task DeleteRecordTemplateAsync(Guid userId, string name);
}
