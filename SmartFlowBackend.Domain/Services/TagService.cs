using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain.Entities;
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

        public async Task AddTagAsync(AddTagRequest request)
        {
            var userId = TestUser.Id;

            var category = await _unitOfWork.Category.GetCategoryByNameAsync(request.Category);

            var tag = new Tag
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                UserId = userId,
                CategoryId = category.Id
            };

            await _unitOfWork.Tag.AddAsync(tag);
            await _unitOfWork.SaveAsync();
        }

        public async Task<List<TagDto>> GetAllTagsByUserIdAsync()
        {
            var tags = await _unitOfWork.Tag.GetAllTagsByUserIdAsync(TestUser.Id);
            return tags.Select(t => new TagDto { Id = t.Id, Name = t.Name }).ToList();
        }
    }
}
