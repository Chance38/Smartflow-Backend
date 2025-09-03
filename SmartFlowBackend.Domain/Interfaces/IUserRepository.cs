using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetUserByIdAsync(Guid userId);
}
