using SmartFlowBackend.Domain.Contracts;

namespace SmartFlowBackend.Domain.Interfaces;

public interface IRecordTemplateService
{
    Task AddRecordTemplateAsync(AddRecordTemplateRequest req, Guid userId);
    Task<List<RecordTemplate>> GetAllRecordTemplatesAsync(Guid userId);
    Task DeleteRecordTemplateAsync(DeleteRecordTemplateRequest req, Guid userId);
}
