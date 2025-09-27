using Domain.Interface;

namespace Domain.Service;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICategoryRepository _repo;

    public CategoryService(IUnitOfWork unitOfWork, ICategoryRepository repo)
    {
        _unitOfWork = unitOfWork;
        _repo = repo;
    }

    public async Task AddCategoryAsync(Guid userId, Contract.Category category)
    {
        var typeEntity = category.Type switch
        {
            Contract.CategoryType.EXPENSE => Entity.CategoryType.EXPENSE,
            Contract.CategoryType.INCOME => Entity.CategoryType.INCOME,
            _ => throw new ArgumentException("Invalid category type")
        };

        var existCategory = await _repo.CheckExistAsync(userId, category.Name, typeEntity);
        if (existCategory != null)
        {
            throw new ArgumentException("Category already exists");
        }

        var categoryEntity = new Entity.Category
        {
            Id = Guid.NewGuid(),
            Name = category.Name,
            Type = typeEntity,
            UserId = userId
        };
        await _repo.AddAsync(categoryEntity);
        await _unitOfWork.SaveAsync();
    }

    public async Task<List<Contract.Category>> GetAllCategoriesAsync(Guid userId)
    {
        var categories = await _repo.GetAllCategoriesAsync(userId);
        return categories.Select(c => new Contract.Category
        {
            Name = c.Name,
            Type = c.Type switch
            {
                Entity.CategoryType.EXPENSE => Contract.CategoryType.EXPENSE,
                Entity.CategoryType.INCOME => Contract.CategoryType.INCOME,
                _ => throw new ArgumentException("data has unexpected category type, this should not happen")
            }
        }).ToList();
    }

    public async Task DeleteCategoryAsync(Guid userId, Contract.Category category)
    {
        var typeEntity = category.Type switch
        {
            Contract.CategoryType.EXPENSE => Entity.CategoryType.EXPENSE,
            Contract.CategoryType.INCOME => Entity.CategoryType.INCOME,
            _ => throw new ArgumentException("Invalid category type")
        };

        await _repo.DeleteAsync(userId, category.Name, typeEntity);
        await _unitOfWork.SaveAsync();
    }

    public async Task UpdateCategoryAsync(Guid userId, Contract.Category oldCategory, Contract.Category newCategory)
    {
        var oldTypeEntity = oldCategory.Type switch
        {
            Contract.CategoryType.EXPENSE => Entity.CategoryType.EXPENSE,
            Contract.CategoryType.INCOME => Entity.CategoryType.INCOME,
            _ => throw new ArgumentException("Invalid category type")
        };

        var existCategory = await _repo.CheckExistAsync(userId, oldCategory.Name, oldTypeEntity);
        if (existCategory == null)
        {
            throw new ArgumentException("Old category does not exist");
        }

        var newTypeEntity = newCategory.Type switch
        {
            Contract.CategoryType.EXPENSE => Entity.CategoryType.EXPENSE,
            Contract.CategoryType.INCOME => Entity.CategoryType.INCOME,
            _ => throw new ArgumentOutOfRangeException()
        };

        var checkNewTag = await _repo.CheckExistAsync(userId, newCategory.Name, newTypeEntity);
        if (checkNewTag != null)
        {
            throw new ArgumentException("New category already exists");
        }

        await _repo.UpdateAsync(existCategory, newCategory.Name, newTypeEntity);
        await _unitOfWork.SaveAsync();
    }
}
