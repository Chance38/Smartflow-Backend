using Domain.Entity;
using Domain.Interface;

namespace Infrastructure.Persistence.Repository;

public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(PostgresDbContext context) : base(context)
    {
    }

    public async Task<Tag?> CheckExistAsync(Guid userId, string name)
    {
        var tag = await FindAsync(t => t.UserId == userId && t.Name == name);
        return tag;
    }

    public async Task<List<Tag>> CheckAllTagsExistAsync(Guid userId, List<string> names)
    {
        var tags = await FindAllAsync(t => t.UserId == userId && names.Contains(t.Name));
        return tags.ToList();
    }

    public async Task<List<Tag>> GetAllTagsAsync(Guid userId)
    {
        var tags = await FindAllAsync(t => t.UserId == userId);
        return tags.ToList();
    }

    public async Task DeleteAsync(Guid userId, string name)
    {
        var deleteTag = await FindAsync(t => t.UserId == userId && t.Name == name);
        if (deleteTag == null)
        {
            throw new ArgumentException("Tag not found");
        }

        await DeleteAsync(deleteTag.Id);
    }

    public async Task UpdateAsync(Tag oldTag, string newName)
    {
        var updateTag = await FindAsync(t => t.UserId == oldTag.UserId && t.Name == oldTag.Name);

        updateTag!.Name = newName;

        await UpdateAsync(updateTag);
    }
}