using SmartFlowBackend.Domain.Contracts;

namespace SmartFlowBackend.Domain.Interfaces
{
    public interface ICategoryService
    {
        Task AddCategoryAsync(AddCategoryRequest request);
        Task<List<Category>> GetAllCategoriesByUserIdAsync();
    }
}
