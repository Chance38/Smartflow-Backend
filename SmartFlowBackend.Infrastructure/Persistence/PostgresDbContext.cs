using Microsoft.EntityFrameworkCore;
using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Infrastructure.Persistence;

public class PostgresDbContext : DbContext
{
    public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options)
    {
    }

    public DbSet<User> User { get; set; }
    public DbSet<Category> Category { get; set; }
    public DbSet<Tag> Tag { get; set; }
    public DbSet<Record> Record { get; set; }
    public DbSet<MonthlySummary> MonthlySummary { get; set; }

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
            .HasOne(r => r.Category)
            .WithMany(c => c.Records)
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

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