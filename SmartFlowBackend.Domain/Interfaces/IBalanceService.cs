using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Domain.Interfaces;

public interface IBalanceService
{
    Task<float> GetBalanceByUserIdAsync(Guid userId);
}
