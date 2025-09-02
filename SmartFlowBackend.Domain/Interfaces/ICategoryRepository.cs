using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetAllCategoriesByUserIdAsync(Guid userId);

    Task<Category> GetCategoryByNameAsync(string name);
}
