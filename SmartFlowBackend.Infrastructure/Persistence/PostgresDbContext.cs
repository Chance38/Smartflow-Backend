using Microsoft.EntityFrameworkCore;
using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Infrastructure.Persistence;

public class PostgresDbContext : DbContext
{
    public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Record> Records { get; set; }
    public DbSet<MonthlyRecordView> MonthlyRecordsView { get; set; }
    public DbSet<BalanceView> BalanceView { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostgresDbContext).Assembly);
    }
}