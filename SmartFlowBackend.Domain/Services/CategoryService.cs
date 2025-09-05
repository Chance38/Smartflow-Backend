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

        public async Task AddCategoryAsync(AddCategoryRequest req, Guid userId)
        {
            var user = await _unitOfWork.User.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var category = new Domain.Entities.Category
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Type = req.Type,
                UserId = userId
            };

            await _unitOfWork.Category.AddAsync(category);
            await _unitOfWork.SaveAsync();
        }

        public async Task<List<Category>> GetAllCategoriesByUserIdAsync(Guid userId)
        {
            var user = await _unitOfWork.User.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var categories = await _unitOfWork.Category.FindAllAsync(c => c.UserId == userId);

            return categories
                .Select(c => new Category
                {
                    Name = c.Name,
                    Type = c.Type
                }).ToList();
        }

        public async Task DeleteCategoryAsync(Guid userId, string categoryName)
        {
            var user = await _unitOfWork.User.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var category = await _unitOfWork.Category.FindAsync(c => c.UserId == userId && c.Name == categoryName);
            if (category == null)
            {
                throw new ArgumentException("Category not found");
            }

            await _unitOfWork.Category.DeleteAsync(category.Id);
            await _unitOfWork.SaveAsync();
        }
    }
}
