using Domain.Entity;

namespace Domain.Interface;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> CheckExistAsync(Guid userId, string name, CategoryType type);
    Task<List<Category>> GetAllCategoriesAsync(Guid userId);
    Task DeleteAsync(Guid userId, string name, CategoryType type);
    Task UpdateAsync(Category oldCategory, string newName, CategoryType newType);
}
