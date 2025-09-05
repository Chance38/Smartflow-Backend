using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain.Interfaces;
using SmartFlowBackend.Domain;
using Microsoft.EntityFrameworkCore;

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
            var user = await _unitOfWork.User.FindAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var existingTag = await _unitOfWork.Tag.FindAsync(t => t.UserId == userId && t.Name == req.Name);
            if (existingTag != null)
            {
                throw new ArgumentException("Tag with the same name already exists.");
            }

            var category = await _unitOfWork.Category.FindAsync(c => c.Name == req.Category && c.UserId == userId);
            if (category == null)
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
            var user = await _unitOfWork.User.FindAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var tags = await _unitOfWork.Tag.FindAllAsync(
                t => t.UserId == userId,
                q => q.Include(t => t.Category)
            );
            return tags
                .Select(t => new Tag
                {
                    Name = t.Name,
                    Category = t.Category.Name
                }).ToList();
        }

        public async Task DeleteTagAsync(Guid userId, string tagName)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.Id == userId);
            if (user == null)
            {
                return;
            }

            var tag = await _unitOfWork.Tag.FindAsync(t => t.UserId == userId && t.Name == tagName);
            if (tag == null)
            {
                return;
            }

            await _unitOfWork.Tag.DeleteAsync(tag.Id);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateTagAsync(UpdateTagRequest request, Guid userId)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var tag = await _unitOfWork.Tag.FindAsync(t => t.UserId == userId && t.Name == request.OldName);
            if (tag == null)
            {
                throw new ArgumentException("Tag not found");
            }

            var category = await _unitOfWork.Category.FindAsync(c => c.Name == request.CategoryName && c.UserId == userId);
            if (category == null)
            {
                throw new ArgumentException("Category not found for the user");
            }

            tag.Name = request.NewName;
            tag.CategoryId = category.Id;

            await _unitOfWork.Tag.UpdateAsync(tag);
            await _unitOfWork.SaveAsync();
        }
    }
}
