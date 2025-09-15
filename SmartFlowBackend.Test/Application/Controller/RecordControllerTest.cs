using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Logging;

using SmartFlowBackend.Application.Controller;
using SmartFlowBackend.Application.SwaggerSetting;
using SmartFlowBackend.Infrastructure.Persistence;
using SmartFlowBackend.Test.Helper;

using Testcontainers.PostgreSql;
using SmartFlowBackend.Domain.Contracts;

namespace SmartFlowBackend.Test.Application.Controller.Record;

public class RecordControllerTest
{
    private PostgreSqlContainer _postgresContainer;
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
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_postgresContainer != null)
            await _postgresContainer.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Record.RemoveRange(db.Record);
        db.Tag.RemoveRange(db.Tag);
        db.Category.RemoveRange(db.Category);
        db.MonthlySummary.RemoveRange(db.MonthlySummary);

        var user = await db.User.FindAsync(TestUser.Id);
        if (user is not null)
        {
            db.User.Remove(user);
        }

        db.User.Add(new Domain.Entities.User
        {
            UserId = TestUser.Id,
            Username = TestUser.Username,
            UserAccount = TestUser.Account,
            UserPassword = TestUser.Password,
            Balance = TestUser.InitialBalance
        });

        await db.SaveChangesAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();
        _client.Dispose();
    }

    [TestCase("food", "expense", Domain.Entities.CategoryType.Expense, "lunch", -50.75f)]
    [TestCase("salary", "income", Domain.Entities.CategoryType.Income, "work", 50.75f)]
    public async Task AddRecord_Should_Return_Ok(string categoryName, string categoryType, Domain.Entities.CategoryType categoryTypeEnum, string tagName, float amount)
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Category.AddRange(new List<Domain.Entities.Category>
        {
            new Domain.Entities.Category
            {
                CategoryName = "food",
                CategoryType = Domain.Entities.CategoryType.Expense,
                UserId = TestUser.Id
            },
            new Domain.Entities.Category
            {
                CategoryName = "salary",
                CategoryType = Domain.Entities.CategoryType.Income,
                UserId = TestUser.Id
            }
        });

        db.Tag.AddRange(new List<Domain.Entities.Tag>
        {
            new Domain.Entities.Tag
            {
                TagName = "lunch",
                UserId = TestUser.Id
            },
            new Domain.Entities.Tag
            {
                TagName = "work",
                UserId = TestUser.Id
            }
        });

        await db.SaveChangesAsync();

        var AddRecordReq =
        @$"
        {{
            ""categoryName"": ""{categoryName}"",
            ""categoryType"": ""{categoryType}"",
            ""tags"": [""{tagName}""],
            ""amount"": 50.75,
            ""date"": ""2023-09-15""
        }}";

        var resp = await _client.PostAsync("/smartflow/v1/record", new StringContent(
            AddRecordReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));

        var record = await db.Record.FirstOrDefaultAsync(r =>
            r.CategoryName == categoryName &&
            r.CategoryType == categoryTypeEnum &&
            r.TagNames.Contains(tagName) &&
            r.Amount == 50.75f &&
            r.Date == DateOnly.Parse("2023-09-15"));
        Assert.IsNotNull(record, "Record should be created in the database");

        var user = await db.User.AsNoTracking().FirstAsync(u => u.UserId == TestUser.Id);
        Assert.That(user!.Balance, Is.EqualTo(10000.0f + amount), "User balance should be updated correctly");
    }

    [TestCase("food", "expense", Domain.Entities.CategoryType.Expense, "lunch", -50.75f)]
    [TestCase("salary", "income", Domain.Entities.CategoryType.Income, "work", 50.75f)]
    public async Task AddRecord_When_MonthHasExistingRecord_Should_Return_Ok_And_UpdateSummary(string categoryName, string categoryType, Domain.Entities.CategoryType categoryTypeEnum, string tagName, float amount)
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Category.AddRange(new List<Domain.Entities.Category>
        {
            new Domain.Entities.Category
            {
                CategoryName = "food",
                CategoryType = Domain.Entities.CategoryType.Expense,
                UserId = TestUser.Id
            },
            new Domain.Entities.Category
            {
                CategoryName = "salary",
                CategoryType = Domain.Entities.CategoryType.Income,
                UserId = TestUser.Id
            }
        });

        db.Tag.AddRange(new List<Domain.Entities.Tag>
        {
            new Domain.Entities.Tag
            {
                TagName = "lunch",
                UserId = TestUser.Id
            },
            new Domain.Entities.Tag
            {
                TagName = "work",
                UserId = TestUser.Id
            }
        });

        var recordDate = DateOnly.Parse("2023-09-15");
        db.MonthlySummary.Add(new Domain.Entities.MonthlySummary
        {
            UserId = TestUser.Id,
            Year = recordDate.Year,
            Month = recordDate.Month,
            Expense = 100.0f,
            Income = 0.00f
        });

        db.User.Update(new Domain.Entities.User
        {
            UserId = TestUser.Id,
            Username = TestUser.Username,
            UserAccount = TestUser.Account,
            UserPassword = TestUser.Password,
            Balance = TestUser.InitialBalance - 100.0f
        });

        await db.SaveChangesAsync();

        var AddRecordReq =
        @$"
        {{
            ""categoryName"": ""{categoryName}"",
            ""categoryType"": ""{categoryType}"",
            ""tags"": [""{tagName}""],
            ""amount"": 50.75,
            ""date"": ""{recordDate.ToString("yyyy-MM-dd")}""
        }}";

        var resp = await _client.PostAsync("/smartflow/v1/record", new StringContent(
            AddRecordReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));

        var content = await resp.Content.ReadAsStringAsync();
        Console.WriteLine(content);

        var record = await db.Record.FirstOrDefaultAsync(r =>
            r.CategoryName == categoryName &&
            r.CategoryType == categoryTypeEnum &&
            r.TagNames.Contains(tagName) &&
            r.Amount == 50.75f &&
            r.Date == DateOnly.Parse("2023-09-15"));
        Assert.IsNotNull(record, "Record should be created in the database");

        var user = await db.User.AsNoTracking().FirstAsync(u => u.UserId == TestUser.Id);
        Assert.That(user!.Balance, Is.EqualTo(10000.0f - 100.0f + amount), "User balance should be updated correctly");
    }

    [Test]
    public async Task AddRecord_When_CategoryName_Is_Not_Exist_Should_Return_BadRequest()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Tag.AddRange(new List<Domain.Entities.Tag>
        {
            new Domain.Entities.Tag
            {
                TagName = "lunch",
                UserId = TestUser.Id
            },
            new Domain.Entities.Tag
            {
                TagName = "dinner",
                UserId = TestUser.Id
            }
        });

        await db.SaveChangesAsync();

        var AddRecordReq =
        """
        {
            "categoryName": "food",
            "categoryType": "expense",
            "tags": ["lunch", "dinner"],
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
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task AddRecord_When_TagNames_Are_Not_Exist_Should_Return_BadRequest()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Category.AddRange(new List<Domain.Entities.Category>
        {
            new Domain.Entities.Category
            {
                CategoryName = "food",
                CategoryType = Domain.Entities.CategoryType.Expense,
                UserId = TestUser.Id
            }
        });

        await db.SaveChangesAsync();

        var AddRecordReq =
        """
        {
            "categoryName": "food",
            "categoryType": "expense",
            "tags": ["lunch", "dinner"],
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
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task GetThisMonthExpenses_Should_Return_Ok()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Record.AddRange(new List<Domain.Entities.Record>
        {
            new Domain.Entities.Record
            {
                CategoryName = "food",
                CategoryType = Domain.Entities.CategoryType.Expense,
                Amount = 50.75f,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = TestUser.Id
            },
            new Domain.Entities.Record
            {
                CategoryName = "transport",
                CategoryType = Domain.Entities.CategoryType.Expense,
                Amount = 20.00f,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = TestUser.Id
            },
            new Domain.Entities.Record
            {
                CategoryName = "salary",
                CategoryType = Domain.Entities.CategoryType.Income,
                Amount = 5000.00f,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = TestUser.Id
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

    [Test]
    public async Task GetMonthRecords_When_Period_Is_1_Should_Return_Ok()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.MonthlySummary.Add(new Domain.Entities.MonthlySummary
        {
            UserId = TestUser.Id,
            Year = DateTime.UtcNow.Year,
            Month = DateTime.UtcNow.Month,
            Expense = 50.75f,
            Income = 0.00f
        });

        await db.SaveChangesAsync();

        var resp = await _client.GetAsync("/smartflow/v1/month-records?period=1");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        Assert.That(content, Does.Contain($"\"year\":{DateTime.UtcNow.Year}"));
        Assert.That(content, Does.Contain($"\"month\":{DateTime.UtcNow.Month}"));
        Assert.That(content, Does.Contain("\"expense\":50.75"));
        Assert.That(content, Does.Contain("\"income\":0"));
    }

    [Test]
    public async Task GetMonthRecords_When_Period_Is_1_But_This_Month_Does_Not_Have_Records_Yet_Should_Return_Ok()
    {
        var resp = await _client.GetAsync("/smartflow/v1/month-records?period=1");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task GetMonthRecords_When_Period_Is_6_Should_Return_Ok()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.MonthlySummary.AddRange(new List<Domain.Entities.MonthlySummary>
        {
            new Domain.Entities.MonthlySummary
            {
                UserId = TestUser.Id,
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.Month,
                Expense = 50.75f,
                Income = 0.00f
            },
            new Domain.Entities.MonthlySummary
            {
                UserId = TestUser.Id,
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.AddMonths(-1).Month,
                Expense = 100.00f,
                Income = 200.00f
            },
            new Domain.Entities.MonthlySummary
            {
                UserId = TestUser.Id,
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.AddMonths(-3).Month,
                Expense = 300.00f,
                Income = 400.00f
            }
        });

        await db.SaveChangesAsync();

        var resp = await _client.GetAsync("/smartflow/v1/month-records?period=6");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        Assert.That(content, Does.Contain($"\"year\":{DateTime.UtcNow.Year}"));
        Assert.That(content, Does.Contain($"\"month\":{DateTime.UtcNow.Month}"));
        Assert.That(content, Does.Contain("\"expense\":50.75"));
        Assert.That(content, Does.Contain("\"income\":0"));
        Assert.That(content, Does.Contain($"\"year\":{DateTime.UtcNow.Year}"));
        Assert.That(content, Does.Contain($"\"month\":{DateTime.UtcNow.AddMonths(-1).Month}"));
        Assert.That(content, Does.Contain("\"expense\":100"));
        Assert.That(content, Does.Contain("\"income\":200"));
        Assert.That(content, Does.Contain($"\"year\":{DateTime.UtcNow.Year}"));
        Assert.That(content, Does.Contain($"\"month\":{DateTime.UtcNow.AddMonths(-2).Month}"));
        Assert.That(content, Does.Contain("\"expense\":0"));
        Assert.That(content, Does.Contain("\"income\":0"));
        Assert.That(content, Does.Contain($"\"year\":{DateTime.UtcNow.Year}"));
        Assert.That(content, Does.Contain($"\"month\":{DateTime.UtcNow.AddMonths(-3).Month}"));
        Assert.That(content, Does.Contain("\"expense\":300"));
        Assert.That(content, Does.Contain("\"income\":400"));
        Assert.That(content, Does.Contain($"\"year\":{DateTime.UtcNow.Year}"));
        Assert.That(content, Does.Contain($"\"month\":{DateTime.UtcNow.AddMonths(-4).Month}"));
        Assert.That(content, Does.Contain("\"expense\":0"));
        Assert.That(content, Does.Contain("\"income\":0"));
        Assert.That(content, Does.Contain($"\"year\":{DateTime.UtcNow.Year}"));
        Assert.That(content, Does.Contain($"\"month\":{DateTime.UtcNow.AddMonths(-5).Month}"));
        Assert.That(content, Does.Contain("\"expense\":0"));
        Assert.That(content, Does.Contain("\"income\":0"));
    }

    [Test]
    public async Task GetMonthRecords_When_Period_Is_Invalid_Should_Return_BadRequest()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.MonthlySummary.Add(new Domain.Entities.MonthlySummary
        {
            UserId = TestUser.Id,
            Year = DateTime.UtcNow.Year,
            Month = DateTime.UtcNow.Month,
            Expense = 50.75f,
            Income = 0.00f
        });

        await db.SaveChangesAsync();

        var resp = await _client.GetAsync("/smartflow/v1/month-records?period=3");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var content = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task GetMonthRecords_When_Period_Is_Not_Query_Should_Return_Ok_With_All_Month_Records()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.MonthlySummary.AddRange(new List<Domain.Entities.MonthlySummary>
        {
            new Domain.Entities.MonthlySummary
            {
                UserId = TestUser.Id,
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.Month,
                Expense = 50.75f,
                Income = 0.00f
            },
            new Domain.Entities.MonthlySummary
            {
                UserId = TestUser.Id,
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.AddMonths(-1).Month,
                Expense = 100.00f,
                Income = 200.00f
            },
            new Domain.Entities.MonthlySummary
            {
                UserId = TestUser.Id,
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.AddMonths(-3).Month,
                Expense = 300.00f,
                Income = 400.00f
            }
        });

        await db.SaveChangesAsync();

        var resp = await _client.GetAsync("/smartflow/v1/month-records");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        Assert.That(content, Does.Contain($"\"year\":{DateTime.UtcNow.Year}"));
        Assert.That(content, Does.Contain($"\"month\":{DateTime.UtcNow.Month}"));
        Assert.That(content, Does.Contain("\"expense\":50.75"));
        Assert.That(content, Does.Contain("\"income\":0"));
        Assert.That(content, Does.Contain($"\"year\":{DateTime.UtcNow.Year}"));
        Assert.That(content, Does.Contain($"\"month\":{DateTime.UtcNow.AddMonths(-1).Month}"));
        Assert.That(content, Does.Contain("\"expense\":100"));
        Assert.That(content, Does.Contain("\"income\":200"));
        Assert.That(content, Does.Contain($"\"year\":{DateTime.UtcNow.Year}"));
        Assert.That(content, Does.Contain($"\"month\":{DateTime.UtcNow.AddMonths(-3).Month}"));
        Assert.That(content, Does.Contain("\"expense\":300"));
        Assert.That(content, Does.Contain("\"income\":400"));
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