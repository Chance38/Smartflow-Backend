using System.Net;
using System.Text;

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

namespace SmartFlowBackend.Test.Application.Controller.Category;

public class CategoryControllerTest
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

        // Ensure DB is clean before each test to avoid cross-test contamination
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Category.RemoveRange(db.Category);
        await db.SaveChangesAsync();
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
        var AddCategoryReq =
        """
        {
            "name": "Test Category",
            "type": "expense"
        }
        """;

        var response = await _client.PostAsync("smartflow/v1/category", new StringContent(
            AddCategoryReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var category = await db.Category.FirstOrDefaultAsync(c => c.CategoryName == "Test Category" && c.Type == Domain.Entities.CategoryType.Expense);
        Assert.IsNotNull(category, "Category should be persisted to DB");
    }

    [Test]
    public async Task AddCategory_When_The_Same_CategoryName_But_Different_Type_Should_Return_Ok()
    {
        var AddCategoryFirstReq =
        """
        {
            "name": "Test Category",
            "type": "expense"
        }
        """;

        var firstResponse = await _client.PostAsync("smartflow/v1/category", new StringContent(
            AddCategoryFirstReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await firstResponse.Content.ReadAsStringAsync();
        Assert.That(firstResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var category = await db.Category.FirstOrDefaultAsync(c => c.CategoryName == "Test Category" && c.Type == Domain.Entities.CategoryType.Expense);
        Assert.IsNotNull(category, "Category should be persisted to DB");

        var AddCategorySecondReq =
        """
        {
            "name": "Test Category",
            "type": "income"
        }
        """;

        var secondResponse = await _client.PostAsync("smartflow/v1/category", new StringContent(
            AddCategorySecondReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(secondResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content2 = await secondResponse.Content.ReadAsStringAsync();
        Assert.That(secondResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        var scope2 = _factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var category2 = await db2.Category.FirstOrDefaultAsync(c => c.CategoryName == "Test Category" && c.Type == Domain.Entities.CategoryType.Income);
        Assert.IsNotNull(category2, "Category should be persisted to DB");
    }

    [Test]
    public async Task AddCategory_When_The_Same_CategoryName_And_The_Same_Type_Should_Return_BadRequest()
    {
        var AddCategoryFirstReq =
        """
        {
            "name": "Test Category",
            "type": "expense"
        }
        """;

        var firstResponse = await _client.PostAsync("smartflow/v1/category", new StringContent(
            AddCategoryFirstReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await firstResponse.Content.ReadAsStringAsync();
        Assert.That(firstResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var category = await db.Category.FirstOrDefaultAsync(c => c.CategoryName == "Test Category");
        Assert.IsNotNull(category, "Category should be persisted to DB");
        Assert.That(category.Type, Is.EqualTo(Domain.Entities.CategoryType.Expense));

        var AddCategorySecondReq =
        """
        {
            "name": "Test Category",
            "type": "expense"
        }
        """;

        var secondResponse = await _client.PostAsync("smartflow/v1/category", new StringContent(
            AddCategorySecondReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(secondResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var content2 = await secondResponse.Content.ReadAsStringAsync();
        Assert.That(secondResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task GetAllCategories_Should_Return_Ok()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Category.AddRange(new List<Domain.Entities.Category>
        {
            new Domain.Entities.Category
            {
                CategoryName = "Test Category",
                Type = Domain.Entities.CategoryType.Expense,
                UserId = TestUser.Id
            },
            new Domain.Entities.Category{
                CategoryName = "Salary",
                Type = Domain.Entities.CategoryType.Income,
                UserId = TestUser.Id
            }
        });

        await db.SaveChangesAsync();

        var c1 = await db.Category.FirstOrDefaultAsync(c => c.CategoryName == "Test Category" && c.Type == Domain.Entities.CategoryType.Expense);
        var c2 = await db.Category.FirstOrDefaultAsync(c => c.CategoryName == "Salary" && c.Type == Domain.Entities.CategoryType.Income);
        Assert.IsNotNull(c1, "Expense category should be persisted to DB");
        Assert.IsNotNull(c2, "Income category should be persisted to DB");

        var response = await _client.GetAsync("smartflow/v1/categories");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        Assert.That(content, Does.Contain("Test Category"));
        Assert.That(content, Does.Contain("Salary"));
        Assert.That(content, Does.Contain("Expense"));
        Assert.That(content, Does.Contain("Income"));
    }

    [Test]
    public async Task DeleteCategory_Should_Return_Ok()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Category.AddRange(new List<Domain.Entities.Category>
        {
            new Domain.Entities.Category
            {
                CategoryName = "Test Category",
                Type = Domain.Entities.CategoryType.Expense,
                UserId = TestUser.Id
            }
        });

        await db.SaveChangesAsync();

        var c1 = await db.Category.FirstOrDefaultAsync(c => c.CategoryName == "Test Category" && c.Type == Domain.Entities.CategoryType.Expense);
        Assert.IsNotNull(c1, "Expense category should be persisted to DB");

        var deleteReq =
        """
        {
            "name": "Test Category",
            "type": "expense"
        }
        """;

        var req = new HttpRequestMessage(HttpMethod.Delete, "smartflow/v1/category")
        {
            Content = new StringContent(deleteReq, Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(req);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var deletedCategory = await db.Category.FirstOrDefaultAsync(c => c.CategoryName == "Test Category" && c.Type == Domain.Entities.CategoryType.Expense);
        Assert.IsNull(deletedCategory, "Category should be deleted from DB");
    }

    [Test]
    public async Task DeleteCategory_When_Category_Is_Not_Exist_Should_Return_Ok()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var c1 = await db.Category.FirstOrDefaultAsync(c => c.CategoryName == "Test Category" && c.Type == Domain.Entities.CategoryType.Expense);
        Assert.IsNull(c1, "Category should not exist in DB");

        var count = await db.Category.CountAsync();
        Assert.That(count, Is.EqualTo(0), "There should be no categories in DB");

        var deleteReq =
        """
        {
            "name": "Test Category",
            "type": "expense"
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
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Category.AddRange(new List<Domain.Entities.Category>
        {
            new Domain.Entities.Category
            {
                CategoryName = "Test Category",
                Type = Domain.Entities.CategoryType.Expense,
                UserId = TestUser.Id
            }
        });

        await db.SaveChangesAsync();

        var c1 = await db.Category.FirstOrDefaultAsync(c => c.CategoryName == "Test Category" && c.Type == Domain.Entities.CategoryType.Expense);
        Assert.IsNotNull(c1, "Expense category should be persisted to DB");

        var updateReq =
        """
        {
            "oldName": "Test Category",
            "newName": "Updated Category",
            "oldType": "expense",
            "newType": "income"
        }
        """;

        var req = new HttpRequestMessage(HttpMethod.Put, "smartflow/v1/category")
        {
            Content = new StringContent(updateReq, Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(req);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var beforeCategory = await db.Category.FirstOrDefaultAsync(c => c.CategoryName == "Test Category" && c.Type == Domain.Entities.CategoryType.Expense);
        Assert.IsNull(beforeCategory, "Old category should not exist in DB");
        var updatedCategory = await db.Category.FirstOrDefaultAsync(c => c.CategoryName == "Updated Category" && c.Type == Domain.Entities.CategoryType.Income);
        Assert.IsNotNull(updatedCategory, "Category should be updated in DB");
    }

    [Test]
    public async Task UpdateCategory_When_Category_Is_Not_Exist_Should_Return_BadRequest()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var c1 = await db.Category.FirstOrDefaultAsync(c => c.CategoryName == "Test Category" && c.Type == Domain.Entities.CategoryType.Expense);
        Assert.IsNull(c1, "Category should not exist in DB");

        var count = await db.Category.CountAsync();
        Assert.That(count, Is.EqualTo(0), "There should be no categories in DB");

        var updateReq =
        """
        {
            "oldName": "Test Category",
            "newName": "Updated Category",
            "oldType": "expense",
            "newType": "income"
        }
        """;

        var req = new HttpRequestMessage(HttpMethod.Put, "smartflow/v1/category")
        {
            Content = new StringContent(updateReq, Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(req);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
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