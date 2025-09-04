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

        public async Task AddCategoryAsync(AddCategoryRequest request)
        {
            var userId = TestUser.Id;

            var category = new Domain.Entities.Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Type = request.Type,
                UserId = userId
            };

            await _unitOfWork.Category.AddAsync(category);
            await _unitOfWork.SaveAsync();
        }

        public async Task<List<Category>> GetAllCategoriesByUserIdAsync()
        {
            var userId = TestUser.Id;
            var categories = await _unitOfWork.Category.GetAllCategoriesByUserIdAsync(userId);

            return categories
                .Select(c => new Category
                {
                    Name = c.Name,
                    Type = c.Type
                }).ToList();
        }
    }
}
