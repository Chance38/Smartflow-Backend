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
using CategoryEntity = Domain.Entity.Category;
using Infrastructure.Persistence;

using Test.Helper;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Test.Application.Controller.Category;

public class CategoryControllerTest
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
    public void SetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();
        _client.Dispose();
    }

    [Test]
    public async Task AddCategory_Should_Return_Ok()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var AddCategoryReq =
        """
        {
            "category": {
                "name": "Test Category",
                "type": "expense"
            }
        }
        """;

        var response = await _client.PostAsync("smartflow/v1/category", new StringContent(
            AddCategoryReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var category = await db.Category.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "Test Category" && c.Type == global::Domain.Entity.CategoryType.EXPENSE);
        Assert.IsNotNull(category, "Category should be persisted to DB");
    }

    [Test]
    public async Task AddCategory_When_The_Same_CategoryName_But_Different_Type_Should_Return_Ok()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var AddCategoryFirstReq =
        """
        {
            "category": {
                "name": "Test Category",
                "type": "expense"
            }
        }
        """;

        var firstResponse = await _client.PostAsync("smartflow/v1/category", new StringContent(
            AddCategoryFirstReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(firstResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var category = await db.Category.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "Test Category" && c.Type == global::Domain.Entity.CategoryType.EXPENSE);
        Assert.IsNotNull(category, "Category should be persisted to DB");

        var AddCategorySecondReq =
        """
        {
            "category": {
                "name": "Test Category",
                "type": "income"
            }
        }
        """;

        var secondResponse = await _client.PostAsync("smartflow/v1/category", new StringContent(
            AddCategorySecondReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(secondResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(secondResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        var scope2 = _factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var category2 = await db2.Category.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "Test Category" && c.Type == global::Domain.Entity.CategoryType.INCOME);
        Assert.IsNotNull(category2, "Category should be persisted to DB");
    }

    [Test]
    public async Task AddCategory_When_The_Same_CategoryName_And_The_Same_Type_Should_Return_BadRequest()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var AddCategoryReq =
        """
        {
            "category": {
                "name": "Test Category",
                "type": "expense"
            }
        }
        """;

        var firstResponse = await _client.PostAsync("smartflow/v1/category", new StringContent(
            AddCategoryReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(firstResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var category = await db.Category.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "Test Category" && c.Type == global::Domain.Entity.CategoryType.EXPENSE);
        Assert.IsNotNull(category, "Category should be persisted to DB");

        var secondResponse = await _client.PostAsync("smartflow/v1/category", new StringContent(
            AddCategoryReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(secondResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(secondResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task GetAllCategories_Should_Return_Ok()
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
                Name = "Test Category",
                Type = global::Domain.Entity.CategoryType.EXPENSE,
                UserId = userId
            },
            new CategoryEntity
            {
                Name = "Test Category 2",
                Type = global::Domain.Entity.CategoryType.INCOME,
                UserId = userId
            }
        });

        await db.SaveChangesAsync();

        var c1 = await db.Category.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "Test Category" && c.Type == global::Domain.Entity.CategoryType.EXPENSE);
        var c2 = await db.Category.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "Test Category 2" && c.Type == global::Domain.Entity.CategoryType.INCOME);
        Assert.IsNotNull(c1, "Expense category should be persisted to DB");
        Assert.IsNotNull(c2, "Income category should be persisted to DB");

        var response = await _client.GetAsync("smartflow/v1/categories");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        Assert.That(content, Does.Contain("Test Category"));
        Assert.That(content, Does.Contain("Test Category 2"));
        Assert.That(content, Does.Contain("expense"));
        Assert.That(content, Does.Contain("income"));

        // 不能使用固定的 Assert 測法，因為回傳的順序是隨機的
        // var jsonResp = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        // var actualCategories = jsonResp["categories"];
        // Assert.That(actualCategories[0].GetProperty("categoryName").GetString(), Is.EqualTo("Test Category"));
        // Assert.That(actualCategories[0].GetProperty("categoryType").GetString(), Is.EqualTo("expense"));
        // Assert.That(actualCategories[1].GetProperty("categoryName").GetString(), Is.EqualTo("Test Category 2"));
        // Assert.That(actualCategories[1].GetProperty("categoryType").GetString(), Is.EqualTo("income"));
    }

    [Test]
    public async Task DeleteCategory_Should_Return_Ok()
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
                Name = "Test Category",
                Type = global::Domain.Entity.CategoryType.EXPENSE,
                UserId = userId
            }
        });

        await db.SaveChangesAsync();

        var c1 = await db.Category.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "Test Category" && c.Type == global::Domain.Entity.CategoryType.EXPENSE);
        Assert.IsNotNull(c1, "Expense category should be persisted to DB");

        var deleteReq =
        """
        {
            "category": {
                "name": "Test Category",
                "type": "expense"
            }
        }
        """;

        var req = new HttpRequestMessage(HttpMethod.Delete, "smartflow/v1/category")
        {
            Content = new StringContent(deleteReq, Encoding.UTF8, "application/json")
        };

        var resp = await _client.SendAsync(req);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var deletedCategory = await db.Category.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "Test Category" && c.Type == global::Domain.Entity.CategoryType.EXPENSE);
        Assert.IsNull(deletedCategory, "Category should be deleted from DB");
    }

    [Test]
    public async Task DeleteCategory_When_Category_Is_Not_Exist_Should_Return_Ok()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var c1 = await db.Category.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "Test Category" && c.Type == global::Domain.Entity.CategoryType.EXPENSE);
        Assert.IsNull(c1, "Category should not exist in DB");

        var count = await db.Category.CountAsync(c => c.UserId == userId);
        Assert.That(count, Is.EqualTo(0), "There should be no categories in DB");

        var deleteReq =
        """
        {
            "category": {
                "name": "Test Category",
                "type": "expense"
            }
        }
        """;

        var req = new HttpRequestMessage(HttpMethod.Delete, "smartflow/v1/category")
        {
            Content = new StringContent(deleteReq, Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(req);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task UpdateCategory_Should_Return_Ok()
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
                Name = "Test Category",
                Type = global::Domain.Entity.CategoryType.EXPENSE,
                UserId = userId
            }
        });

        await db.SaveChangesAsync();

        var c1 = await db.Category.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "Test Category" && c.Type == global::Domain.Entity.CategoryType.EXPENSE);
        Assert.IsNotNull(c1, "Expense category should be persisted to DB");

        var updateReq =
        """
        {
            "oldCategory": {
                "name": "Test Category",
                "type": "expense"
            },
            "newCategory": {
                "name": "Updated Category",
                "type": "income"
            }
        }
        """;

        var resp = await _client.PutAsync("smartflow/v1/category", new StringContent(
            updateReq,
            Encoding.UTF8,
            "application/json"
        ));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resp.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        var oldCategory = await db.Category.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "Test Category" && c.Type == global::Domain.Entity.CategoryType.EXPENSE);
        Assert.IsNull(oldCategory, "Old category should not exist in DB");

        var updatedCategory = await db.Category.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "Updated Category" && c.Type == global::Domain.Entity.CategoryType.INCOME);
        Assert.IsNotNull(updatedCategory, "Category should be updated in DB");
    }

    [Test]
    public async Task UpdateCategory_When_Category_Is_Not_Exist_Should_Return_BadRequest()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var c1 = await db.Category.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "Test Category" && c.Type == global::Domain.Entity.CategoryType.EXPENSE);
        Assert.IsNull(c1, "Category should not exist in DB");

        var count = await db.Category.CountAsync(c => c.UserId == userId);
        Assert.That(count, Is.EqualTo(0), "There should be no categories in DB");

        var updateReq =
        """
        {
            "oldCategory": {
                "name": "Test Category",
                "type": "expense"
            },
            "newCategory": {
                "name": "Updated Category",
                "type": "income"
            }
        }
        """;

        var resp = await _client.PutAsync("smartflow/v1/category", new StringContent(
            updateReq,
            Encoding.UTF8,
            "application/json"
        ));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(resp.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeLogger<CategoryController> fakeLogger = null!;

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            fakeLogger = new FakeLogger<CategoryController>();
            services.AddSingleton<ILogger<CategoryController>>(fakeLogger);

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PostgresDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            var conn = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres_Connection")!;
            services.AddDbContext<PostgresDbContext>(options => options.UseNpgsql(conn));
        });
    }
}