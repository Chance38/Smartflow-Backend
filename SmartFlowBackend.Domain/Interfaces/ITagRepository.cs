using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Domain.Interfaces;

public interface ITagRepository : IRepository<Tag>
{
    Task<IEnumerable<Tag>> GetAllTagsByUserIdAsync(Guid userId);

    Task<Tag> GetTagByNameAsync(string name);
}
