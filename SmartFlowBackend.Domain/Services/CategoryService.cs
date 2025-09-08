using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain.Interfaces;
using SmartFlowBackend.Domain;

namespace SmartFlowBackend.Domain.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddCategoryAsync(AddCategoryRequest request, Guid userId)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var existingCategory = await _unitOfWork.Category.FindAsync(c => c.UserId == userId && c.CategoryName == request.Name && c.Type == request.Type);
            if (existingCategory != null)
            {
                throw new ArgumentException("Category with the same name already exists.");
            }

            var category = new Domain.Entities.Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = request.Name,
                Type = request.Type,
                UserId = userId
            };

            await _unitOfWork.Category.AddAsync(category);
            await _unitOfWork.SaveAsync();
        }

        public async Task<List<Category>> GetAllCategoriesByUserIdAsync(Guid userId)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var categories = await _unitOfWork.Category.FindAllAsync(c => c.UserId == userId);

            return categories
                .Select(c => new Category
                {
                    Name = c.CategoryName,
                    Type = c.Type
                }).ToList();
        }

        public async Task DeleteCategoryAsync(Guid userId, string categoryName)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
            if (user == null)
            {
                return;
            }

            var category = await _unitOfWork.Category.FindAsync(c => c.UserId == userId && c.CategoryName == categoryName);
            if (category == null)
            {
                return;
            }

            await _unitOfWork.Category.DeleteAsync(category.CategoryId);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateCategoryAsync(UpdateCategoryRequest req, Guid userId)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var category = await _unitOfWork.Category.FindAsync(c => c.UserId == userId && c.CategoryName == req.OldName);
            if (category == null)
            {
                throw new ArgumentException("Category not found");
            }

            category.CategoryName = req.NewName;

            await _unitOfWork.Category.UpdateAsync(category);
            await _unitOfWork.SaveAsync();
        }
    }
}
