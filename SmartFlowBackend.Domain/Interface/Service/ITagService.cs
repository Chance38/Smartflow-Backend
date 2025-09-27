using Domain.Contract;

namespace Domain.Interface;

public interface ITagService
{
    Task AddTagAsync(Guid userId, Tag tag);
    Task<List<Tag>> GetAllTagsAsync(Guid userId);
    Task DeleteTagAsync(Guid userId, Tag tag);
    Task UpdateTagAsync(Guid userId, Tag oldTag, Tag newTag);
}
