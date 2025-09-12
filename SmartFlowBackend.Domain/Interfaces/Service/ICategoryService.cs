using SmartFlowBackend.Domain.Contracts;

namespace SmartFlowBackend.Domain.Interfaces
{
    public interface ICategoryService
    {
        Task AddCategoryAsync(AddCategoryRequest request, Guid userId);

        Task<List<Category>> GetAllCategoriesByUserIdAsync(Guid userId);

        Task DeleteCategoryAsync(DeleteCategoryRequest request, Guid userId);

        Task UpdateCategoryAsync(UpdateCategoryRequest request, Guid userId);
    }
}
