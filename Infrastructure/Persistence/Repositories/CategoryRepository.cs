using Domain.Entity;
using Domain.Interface;

namespace Infrastructure.Persistence.Repository;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(PostgresDbContext context) : base(context)
    {
    }

    public async Task<Category?> CheckExistAsync(Guid userId, string name, CategoryType type)
    {
        var category = await FindAsync(c => c.UserId == userId && c.Name == name && c.Type == type);
        return category;
    }

    public async Task<List<Category>> GetAllCategoriesAsync(Guid userId)
    {
        var categories = await FindAllAsync(c => c.UserId == userId);
        return categories.ToList();
    }

    public async Task DeleteAsync(Guid userId, string name, CategoryType type)
    {
        var deleteCategory = await FindAsync(c => c.UserId == userId && c.Name == name && c.Type == type);
        if (deleteCategory == null)
        {
            throw new ArgumentException("Category not found");
        }

        await DeleteAsync(deleteCategory.Id);
    }

    public async Task UpdateAsync(Category oldCategory, string newName, CategoryType newType)
    {
        var updateCategory = await FindAsync(c => c.UserId == oldCategory.UserId && c.Name == oldCategory.Name && c.Type == oldCategory.Type);

        updateCategory!.Name = newName;
        updateCategory!.Type = newType;

        await UpdateAsync(updateCategory);
    }
}