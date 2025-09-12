using Microsoft.EntityFrameworkCore;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;

namespace SmartFlowBackend.Infrastructure.Persistence.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(PostgresDbContext context) : base(context)
    {
    }
}