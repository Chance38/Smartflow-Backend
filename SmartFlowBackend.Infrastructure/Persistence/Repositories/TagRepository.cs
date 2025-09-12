using Microsoft.EntityFrameworkCore;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;

namespace SmartFlowBackend.Infrastructure.Persistence.Repositories;

public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(PostgresDbContext context) : base(context)
    {
    }
}