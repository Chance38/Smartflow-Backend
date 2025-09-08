using SmartFlowBackend.Domain.Contracts;

namespace SmartFlowBackend.Domain.Interfaces
{
    public interface ITagService
    {
        Task AddTagAsync(AddTagRequest request, Guid userId);
        Task<List<Tag>> GetAllTagsByUserIdAsync(Guid userId);
        Task DeleteTagAsync(Guid userId, string tagName);
        Task UpdateTagAsync(UpdateTagRequest request, Guid userId);
    }
}
