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
    public DbSet<MonthlySummary> MonthlySummaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToTable("User");
        modelBuilder.Entity<Category>().ToTable("Category");
        modelBuilder.Entity<Record>().ToTable("Record");
        modelBuilder.Entity<Tag>().ToTable("Tag");
        modelBuilder.Entity<MonthlySummary>().ToTable("MonthlySummary");

        modelBuilder.Entity<MonthlySummary>()
            .HasOne(ms => ms.User)
            .WithMany()
            .HasForeignKey(ms => ms.UserId);

        modelBuilder.Entity<Record>()
            .HasIndex(r => r.Date);

        modelBuilder.Entity<Record>()
            .HasMany(r => r.Tags)
            .WithMany(t => t.Records);

        modelBuilder.Entity<Record>()
            .Property(r => r.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Category>()
            .Property(c => c.Type)
            .HasConversion<string>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostgresDbContext).Assembly);
    }
}