using SmartFlowBackend.Domain.Contracts;
using SmartFlowBackend.Domain.Interfaces;

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
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var existingTag = await _unitOfWork.Tag.FindAsync(t => t.UserId == userId && t.TagName == req.TagName);
            if (existingTag != null)
            {
                throw new ArgumentException("Tag with the same name already exists.");
            }

            var tag = new Domain.Entities.Tag
            {
                TagId = Guid.NewGuid(),
                TagName = req.TagName,
                UserId = userId
            };

            await _unitOfWork.Tag.AddAsync(tag);
            await _unitOfWork.SaveAsync();
        }

        public async Task<List<Tag>> GetAllTagsByUserIdAsync(Guid userId)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var tags = await _unitOfWork.Tag.FindAllAsync(
                t => t.UserId == userId
            );
            return tags
                .Select(t => new Tag
                {
                    TagName = t.TagName
                }).ToList();
        }

        public async Task DeleteTagAsync(Guid userId, string tagName)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
            if (user == null)
            {
                return;
            }

            var tag = await _unitOfWork.Tag.FindAsync(t => t.UserId == userId && t.TagName == tagName);
            if (tag == null)
            {
                return;
            }

            await _unitOfWork.Tag.DeleteAsync(tag.TagId);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateTagAsync(UpdateTagRequest request, Guid userId)
        {
            var user = await _unitOfWork.User.FindAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var tag = await _unitOfWork.Tag.FindAsync(t => t.UserId == userId && t.TagName == request.OldName);
            if (tag == null)
            {
                throw new ArgumentException("Tag not found");
            }

            tag.TagName = request.NewName;

            await _unitOfWork.Tag.UpdateAsync(tag);
            await _unitOfWork.SaveAsync();
        }
    }
}
