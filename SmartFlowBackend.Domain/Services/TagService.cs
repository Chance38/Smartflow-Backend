using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain.Interfaces;
using SmartFlowBackend.Domain;

namespace SmartFlowBackend.Domain.Services
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TagService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddTagAsync(AddTagRequest req, Guid userId)
        {
            var user = await _unitOfWork.User.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var category = await _unitOfWork.Category.GetCategoryByNameAsync(req.Category);
            if (category == null || category.UserId != userId)
            {
                throw new ArgumentException("Category not found for the user");
            }

            var tag = new Domain.Entities.Tag
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                CategoryId = category.Id,
                UserId = userId
            };

            await _unitOfWork.Tag.AddAsync(tag);
            await _unitOfWork.SaveAsync();
        }

        public async Task<List<Tag>> GetAllTagsByUserIdAsync(Guid userId)
        {
            var user = await _unitOfWork.User.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var tags = await _unitOfWork.Tag.FindAllAsync(t => t.UserId == userId);
            return tags
                .Select(t => new Tag
                {
                    Name = t.Name,
                    Category = t.Category.Name
                }).ToList();
        }
    }
}
