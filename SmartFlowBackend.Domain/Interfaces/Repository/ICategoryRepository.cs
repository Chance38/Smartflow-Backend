using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<List<Category>> GetAllCategoriesByUserIdAsync(Guid userId);
    Task<Category> GetCategoryByNameAsync(string name);
}
