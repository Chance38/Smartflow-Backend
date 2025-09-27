using Domain.Contract;
using Domain.Interface;

namespace Domain.Service
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITagRepository _repo;

        public TagService(IUnitOfWork unitOfWork, ITagRepository repo)
        {
            _unitOfWork = unitOfWork;
            _repo = repo;
        }

        public async Task AddTagAsync(Guid userId, Contract.Tag tag)
        {
            var existTag = await _repo.CheckExistAsync(userId, tag.Name);
            if (existTag != null)
            {
                throw new ArgumentException("Category already exists");
            }

            var tagEntity = new Entity.Tag
            {
                Id = Guid.NewGuid(),
                Name = tag.Name,
                UserId = userId
            };
            await _repo.AddAsync(tagEntity);
            await _unitOfWork.SaveAsync();
        }

        public async Task<List<Contract.Tag>> GetAllTagsAsync(Guid userId)
        {
            var tags = await _repo.GetAllTagsAsync(userId);
            return tags.Select(c => new Contract.Tag
            {
                Name = c.Name,
            }).ToList();
        }

        public async Task DeleteTagAsync(Guid userId, Contract.Tag tag)
        {
            await _repo.DeleteAsync(userId, tag.Name);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateTagAsync(Guid userId, Tag oldTag, Tag newTag)
        {
            var existTag = await _repo.CheckExistAsync(userId, oldTag.Name);
            if (existTag == null)
            {
                throw new ArgumentException("Old tag does not exist");
            }

            var checkNewTag = await _repo.CheckExistAsync(userId, newTag.Name);
            if (checkNewTag != null)
            {
                throw new ArgumentException("New tag already exists");
            }

            await _repo.UpdateAsync(existTag, newTag.Name);
            await _unitOfWork.SaveAsync();
        }
    }
}
