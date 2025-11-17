using Application.Contract;

namespace Application.Interface.Service;

public interface ICategoryService
{
    Task AddCategoryAsync(Guid userId, Category category);

    Task<List<Category>> GetAllCategoriesAsync(Guid userId);

    Task DeleteCategoryAsync(Guid userId, Category category);

    Task UpdateCategoryAsync(Guid userId, Category oldCategory, Category newCategory);
}
