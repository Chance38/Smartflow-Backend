using System.Net;
using System.Text;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Logging;

using Presentation.Controller;
using Application.Contract;
using Domain.Entity;
using RecordEntity = Domain.Entity.Record;
using CategoryEntity = Domain.Entity.Category;
using TagEntity = Domain.Entity.Tag;
using BalanceEntity = Domain.Entity.Balance;
using Infrastructure.Persistence;

using Test.Helper;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Test.Application.Controller.Record;

public class RecordControllerTest
{
    private PostgreSqlContainer _postgresContainer;
    private RabbitMqContainer _rabbitMqContainer;
    private CustomWebApplicationFactory _factory;
    private HttpClient _client;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        Console.WriteLine("Starting PostgreSQL container...");
        _postgresContainer = TestHelper.CreatePostgreSqlContainer();
        await _postgresContainer.StartAsync();
        Console.WriteLine("PostgreSQL container started.");

        var postgresUrl = _postgresContainer.GetConnectionString();
        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres_Connection", postgresUrl);

        Console.WriteLine("Starting RabbitMQ container...");
        _rabbitMqContainer = TestHelper.CreateRabbitMQContainer();
        await _rabbitMqContainer.StartAsync();
        Console.WriteLine("RabbitMQ container started.");

        var rabbitMqPort = _rabbitMqContainer.GetMappedPublicPort(5672);
        Environment.SetEnvironmentVariable("RABBITMQ_HOST", _rabbitMqContainer.Hostname);
        Environment.SetEnvironmentVariable("RABBITMQ_PORT", rabbitMqPort.ToString());
        Environment.SetEnvironmentVariable("RABBITMQ_USERNAME", "testuser");
        Environment.SetEnvironmentVariable("RABBITMQ_PASSWORD", "testpass");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_postgresContainer != null)
            await _postgresContainer.DisposeAsync();

        if (_rabbitMqContainer != null)
            await _rabbitMqContainer.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        await db.Database.MigrateAsync();
        await db.SaveChangesAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();
        _client.Dispose();
    }

    [TestCase("food", "expense", global::Domain.Entity.CategoryType.EXPENSE, "lunch", -50.75f)]
    [TestCase("salary", "income", global::Domain.Entity.CategoryType.INCOME, "work", 50.75f)]
    public async Task AddRecord_Should_Return_Ok(string categoryName, string categoryType, global::Domain.Entity.CategoryType categoryTypeEnum, string tagName, float amount)
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Category.AddRange(new List<CategoryEntity>
        {
            new CategoryEntity
            {
                Name = "food",
                Type = global::Domain.Entity.CategoryType.EXPENSE,
                UserId = userId
            },
            new CategoryEntity
            {
                Name = "salary",
                Type = global::Domain.Entity.CategoryType.INCOME,
                UserId = userId
            }
        });

        db.Tag.AddRange(new List<TagEntity>
        {
            new TagEntity
            {
                Name = "lunch",
                UserId = userId
            },
            new TagEntity
            {
                Name = "work",
                UserId = userId
            }
        });

        db.Balance.Add(new BalanceEntity
        {
            UserId = userId,
            Amount = 10000.0f
        });

        await db.SaveChangesAsync();

        var AddRecordReq =
        $$"""
        {
            "category": {
                "name": "{{categoryName}}",
                "type": "{{categoryType}}"
            },
            "tags": [
                {
                    "name": "{{tagName}}"
                }
            ],
            "amount": {{Math.Abs(amount)}},
            "date": "2025-09-15"
        }
        """;

        var resp = await _client.PostAsync("/smartflow/v1/record", new StringContent(
            AddRecordReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resp.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        var record = await db.Record.FirstOrDefaultAsync(r =>
            r.UserId == userId &&
            r.CategoryName == categoryName &&
            r.Type == categoryTypeEnum &&
            r.TagNames.Contains(tagName) &&
            r.Amount == Math.Abs(amount) &&
            r.Date == DateOnly.FromDateTime(DateTime.Parse("2025-09-15")));
        Assert.IsNotNull(record, "Record should be created in the database");
    }

    [TestCase("food", "expense", global::Domain.Entity.CategoryType.EXPENSE, "lunch", -50.75f)]
    [TestCase("salary", "income", global::Domain.Entity.CategoryType.INCOME, "work", 50.75f)]
    public async Task Balance_And_MonthlySummary_Table_Should_trigger_When_add_new_record(string categoryName, string categoryType, global::Domain.Entity.CategoryType categoryTypeEnum, string tagName, float amount)
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Category.AddRange(new List<CategoryEntity>
        {
            new CategoryEntity
            {
                Name = "food",
                Type = global::Domain.Entity.CategoryType.EXPENSE,
                UserId = userId
            },
            new CategoryEntity
            {
                Name = "salary",
                Type = global::Domain.Entity.CategoryType.INCOME,
                UserId = userId
            }
        });

        db.Tag.AddRange(new List<TagEntity>
        {
            new TagEntity
            {
                Name = "lunch",
                UserId = userId
            },
            new TagEntity
            {
                Name = "work",
                UserId = userId
            }
        });

        var recordDate = DateOnly.Parse("2023-09-15");

        db.Balance.Add(new BalanceEntity
        {
            UserId = userId,
            Amount = 10000.0f
        });

        await db.SaveChangesAsync();

        var AddRecordReq =
        $$"""
        {
            "category": {
                "name": "{{categoryName}}",
                "type": "{{categoryType}}"
            },
            "tags": [
                {
                    "name": "{{tagName}}"
                }
            ],
            "amount": {{Math.Abs(amount)}},
            "date": "{{recordDate.ToString("yyyy-MM-dd")}}"
        }
        """;

        var resp = await _client.PostAsync("/smartflow/v1/record", new StringContent(
            AddRecordReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resp.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        var record = await db.Record.FirstOrDefaultAsync(r =>
            r.UserId == userId &&
            r.CategoryName == categoryName &&
            r.Type == categoryTypeEnum &&
            r.TagNames.Contains(tagName) &&
            r.Amount == Math.Abs(amount) &&
            r.Date == recordDate);
        Assert.IsNotNull(record, "Record should be created in the database");

        var balance = await db.Balance.FirstAsync(b =>
            b.UserId == userId);
        await db.Entry(balance).ReloadAsync();
        Assert.That(balance.Amount, Is.EqualTo(10000.0f + amount));

        scope = _factory.Services.CreateScope();
        db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var summary = await db.MonthlySummary.FirstAsync(s =>
            s.UserId == userId &&
            s.Year == recordDate.Year &&
            s.Month == recordDate.Month);

        if (categoryTypeEnum == global::Domain.Entity.CategoryType.EXPENSE)
        {
            Assert.That(summary.Expense, Is.EqualTo(Math.Abs(amount)));
        }
        else if (categoryTypeEnum == global::Domain.Entity.CategoryType.INCOME)
        {
            Assert.That(summary.Income, Is.EqualTo(amount));
        }
    }

    [Test]
    public async Task AddRecord_When_CategoryName_Is_Not_Exist_Should_Return_BadRequest()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Tag.AddRange(new List<TagEntity>
        {
            new TagEntity
            {
                Name = "lunch",
                UserId = userId
            },
            new TagEntity
            {
                Name = "dinner",
                UserId = userId
            }
        });

        await db.SaveChangesAsync();

        var AddRecordReq =
        """
        {
            "category": {
                "name": "food",
                "type": "expense"
            },
            "tags": [
                {
                    "name": "lunch"
                },
                {
                    "name": "dinner"
                }
            ],
            "amount": 50.75,
            "date": "2023-09-15"
        }
        """;

        var resp = await _client.PostAsync("/smartflow/v1/record", new StringContent(
            AddRecordReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(resp.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task AddRecord_When_TagNames_Are_Not_Exist_Should_Return_BadRequest()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Category.AddRange(new List<CategoryEntity>
        {
            new CategoryEntity
            {
                Name = "food",
                Type = global::Domain.Entity.CategoryType.EXPENSE,
                UserId = userId
            }
        });

        await db.SaveChangesAsync();

        var AddRecordReq =
        """
        {
            "category": {
                "name": "food",
                "type": "expense"
            },
            "tags": [
                {
                    "name": "lunch"
                },
                {
                    "name": "dinner"
                }
            ],
            "amount": 50.75,
            "date": "2023-09-15"
        }
        """;

        var resp = await _client.PostAsync("/smartflow/v1/record", new StringContent(
            AddRecordReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(resp.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task AddRecord_Test()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Category.AddRange(new List<CategoryEntity>
        {
            new CategoryEntity
            {
                Name = "泡澡",
                Type = global::Domain.Entity.CategoryType.EXPENSE,
                UserId = userId
            }
        });

        db.Balance.Add(new BalanceEntity
        {
            UserId = userId,
            Amount = 10000.0f
        });

        await db.SaveChangesAsync();

        var AddRecordReq =
        """
        {
            "category": {
                "name": "泡澡",
                "type": "expense"
            },
            "tags": [],
            "amount": 150,
            "date": "2025-09-15"
        }
        """;

        var resp = await _client.PostAsync("/smartflow/v1/record", new StringContent(
            AddRecordReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resp.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task GetThisMonthExpenses_Should_Return_Ok()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Record.AddRange(new List<RecordEntity>
        {
            new RecordEntity
            {
                CategoryName = "food",
                Type = global::Domain.Entity.CategoryType.EXPENSE,
                Amount = 50.75f,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = userId
            },
            new RecordEntity
            {
                CategoryName = "transport",
                Type = global::Domain.Entity.CategoryType.EXPENSE,
                Amount = 20.00f,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = userId
            },
            new RecordEntity
            {
                CategoryName = "salary",
                Type = global::Domain.Entity.CategoryType.INCOME,
                Amount = 5000.00f,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = userId
            }
        });

        await db.SaveChangesAsync();

        var resp = await _client.GetAsync("/smartflow/v1/expenses");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        Assert.That(content, Does.Contain("\"categoryName\":\"food\""));
        Assert.That(content, Does.Contain("\"amount\":50.75"));
        Assert.That(content, Does.Contain("\"categoryName\":\"transport\""));
        Assert.That(content, Does.Contain("\"amount\":20"));
        Assert.That(content, Does.Not.Contain("\"categoryName\":\"salary\""));
        Assert.That(content, Does.Not.Contain("\"amount\":5000"));
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeLogger<RecordController> fakeLogger = null!;

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            fakeLogger = new FakeLogger<RecordController>();
            services.AddSingleton<ILogger<RecordController>>(fakeLogger);

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PostgresDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            var conn = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres_Connection")!;
            services.AddDbContext<PostgresDbContext>(options => options.UseNpgsql(conn));
        });
    }
}