using Domain.Entity;
using Domain.Interface;

namespace Infrastructure.Persistence.Repository;

public class RecordTemplateRepository : Repository<RecordTemplate>, IRecordTemplateRepository
{
    public RecordTemplateRepository(PostgresDbContext context) : base(context)
    {
    }

    public async Task<RecordTemplate?> CheckExistAsync(Guid userId, string name)
    {
        var recordTemplate = await FindAsync(rt => rt.UserId == userId && rt.Name == name);
        return recordTemplate;
    }

    public async Task<List<RecordTemplate>> GetAllRecordTemplatesAsync(Guid userId)
    {
        var recordTemplates = await FindAllAsync(rt => rt.UserId == userId);
        return recordTemplates.ToList();
    }

    public async Task DeleteAsync(Guid userId, string name)
    {
        var deleteRecordTemplate = await FindAsync(rt => rt.UserId == userId && rt.Name == name);
        if (deleteRecordTemplate == null)
        {
            throw new ArgumentException("RecordTemplate not found");
        }

        await DeleteAsync(deleteRecordTemplate.Id);
    }
}