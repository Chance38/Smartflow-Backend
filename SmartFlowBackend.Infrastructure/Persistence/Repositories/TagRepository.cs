using Microsoft.EntityFrameworkCore;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;

namespace SmartFlowBackend.Infrastructure.Persistence.Repositories;

public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(PostgresDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Tag>> GetAllTagsByUserIdAsync(Guid userId)
    {
        return await _context.Tag
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task<Tag> GetTagByNameAsync(string name)
    {
        var tag = await _context.Tag.FirstOrDefaultAsync(t => t.TagName == name);
        if (tag == null)
            throw new InvalidOperationException($"Tag with name '{name}' not found.");
        return tag;
    }
}