using Domain.Entity;

namespace Domain.Interface;

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> CheckExistAsync(Guid userId, string name);
    Task<List<Tag>> CheckAllTagsExistAsync(Guid userId, List<string> names);
    Task<List<Tag>> GetAllTagsAsync(Guid userId);
    Task DeleteAsync(Guid userId, string name);
    Task UpdateAsync(Tag oldTag, string newName);
}
