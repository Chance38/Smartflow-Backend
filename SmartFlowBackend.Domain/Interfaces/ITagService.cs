using SmartFlowBackend.Domain.Contracts;

namespace SmartFlowBackend.Domain.Interfaces
{
    public interface ITagService
    {
        Task AddTagAsync(AddTagRequest request);
        Task<List<TagDto>> GetAllTagsByUserIdAsync();
    }
}
