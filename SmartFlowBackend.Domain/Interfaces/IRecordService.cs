using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Domain.Interfaces;

public interface IRecordService
{
    Task AddRecordAsync(AddRecordRequest request, Guid userId);
    Task<GetThisMonthRecordResponse> GetThisMonthRecordsAsync(Guid userId);
    Task<GetLastSixMonthRecordsResponse> GetLastSixMonthRecordsAsync(Guid userId);
    Task<GetAllMonthRecordsResponse> GetAllMonthRecordsAsync(Guid userId);
}
