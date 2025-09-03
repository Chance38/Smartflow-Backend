using Microsoft.EntityFrameworkCore;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;

namespace SmartFlowBackend.Infrastructure.Persistence.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(PostgresDbContext context) : base(context)
    {
    }

    public async Task<List<Category>> GetAllCategoriesByUserIdAsync(Guid userId)
    {
        return await _context.Categories
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task<Category> GetCategoryByNameAsync(string name)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == name);
        if (category == null)
        {
            throw new InvalidOperationException($"Category with name '{name}' not found.");
        }
        return category;
    }
}