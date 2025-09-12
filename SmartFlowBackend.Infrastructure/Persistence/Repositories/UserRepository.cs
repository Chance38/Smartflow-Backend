using Microsoft.EntityFrameworkCore;
using SmartFlowBackend.Domain.Entities;
using SmartFlowBackend.Domain.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace SmartFlowBackend.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(PostgresDbContext context) : base(context)
    {
    }
}