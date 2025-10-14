using System.Net;
using System.Text;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Logging;

using Application.Controller;
using Domain.Entity;
using TemplateEntity = Domain.Entity.RecordTemplate;
using Infrastructure.Persistence;

using Test.Helper;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Test.Application.Controller.RecordTemplate;

public class RecordTemplateControllerTest
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
    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();
        _client.Dispose();
    }

    [Test]
    public async Task AddRecordTemplate_Should_Returns_Ok()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var AddRecordTemplateReq =
        """
        {
            "recordTemplate":{
                "name": "Test Template",
                "categoryName": "food",
                "categoryType": "expense",
                "tags": [
                    {"name": "tag1"},
                    {"name": "tag2"}
                ],
                "amount": 100.0
            }
        }
        """;

        var resp = await _client.PostAsync("/smartflow/v1/record-template", new StringContent(
            AddRecordTemplateReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var recordTemplates = await db.RecordTemplate.FirstOrDefaultAsync(rt =>
            rt.Name == "Test Template" &&
            rt.CategoryName == "food" &&
            rt.CategoryType == CategoryType.EXPENSE &&
            rt.TagNames.Contains("tag1") &&
            rt.TagNames.Contains("tag2") &&
            rt.Amount == 100.0 &&
            rt.UserId == userId
        );

        Assert.IsNotNull(recordTemplates, "RecordTemplate should be added to the database");
    }

    [Test]
    public async Task AddRecordTemplate_Only_CategoryNameIsEmpty_Should_Returns_Ok()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var AddRecordTemplateReq =
        """
        {
            "recordTemplate":{
                "name": "Test Template",
                "categoryType": "expense",
                "tags": [
                    {"name": "tag1"},
                    {"name": "tag2"}
                ],
                "amount": 100.0
            }
        }
        """;

        var resp = await _client.PostAsync("/smartflow/v1/record-template", new StringContent(
            AddRecordTemplateReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var recordTemplates = await db.RecordTemplate.FirstOrDefaultAsync(rt =>
            rt.Name == "Test Template" &&
            rt.CategoryType == CategoryType.EXPENSE &&
            rt.TagNames.Contains("tag1") &&
            rt.TagNames.Contains("tag2") &&
            rt.Amount == 100.0 &&
            rt.UserId == userId
        );

        Assert.IsNotNull(recordTemplates, "RecordTemplate should be added to the database");
    }

    [Test]
    public async Task AddRecordTemplate_Only_TagsIsEmpty_Should_Returns_Ok()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var AddRecordTemplateReq =
        """
        {
            "recordTemplate":{
                "name": "Test Template",
                "categoryName": "salary",
                "categoryType": "income",
                "amount": 100.0
            }
        }
        """;

        var resp = await _client.PostAsync("/smartflow/v1/record-template", new StringContent(
            AddRecordTemplateReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var recordTemplates = await db.RecordTemplate.FirstOrDefaultAsync(rt =>
            rt.Name == "Test Template" &&
            rt.CategoryName == "salary" &&
            rt.CategoryType == CategoryType.INCOME &&
            rt.Amount == 100.0 &&
            rt.UserId == userId
        );

        Assert.IsNotNull(recordTemplates, "RecordTemplate should be added to the database");
    }

    [Test]
    public async Task AddRecordTemplate_When_TemplateNameAlreadyExists_Should_Return_BadRequest()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.RecordTemplate.Add(new TemplateEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Template",
            CategoryName = "food",
            CategoryType = CategoryType.EXPENSE,
            TagNames = new List<string> { "tag1", "tag2" },
            Amount = 100.0f,
            UserId = userId
        });

        db.SaveChanges();

        var AddRecordTemplateReq =
        """
        {
            "recordTemplate":{
                "name": "Test Template",
                "categoryName": "food",
                "categoryType": "expense",
                "tags": [
                    {"name": "tag1"},
                    {"name": "tag2"}
                ],
                "amount": 100.0
            }
        }
        """;

        var resp = await _client.PostAsync("/smartflow/v1/record-template", new StringContent(
            AddRecordTemplateReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task AddRecordTemplate_When_CategoryNameIsEmpty_And_TagsIsEmpty_Should_Return_BadRequest()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var AddRecordTemplateReq =
        """
        {
            "recordTemplate":{
                "name": "Test Template",
                "categoryType": "expense",
                "amount": 100.0
            }
        }
        """;

        var resp = await _client.PostAsync("/smartflow/v1/record-template", new StringContent(
            AddRecordTemplateReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task GetAllRecordTemplates_Should_Returns_Ok_With_RecordTemplates()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.RecordTemplate.AddRange(new List<TemplateEntity>
        {
            new TemplateEntity
            {
                Id = Guid.NewGuid(),
                Name = "Template 1",
                CategoryName = "food",
                CategoryType = CategoryType.EXPENSE,
                TagNames = new List<string> { "tag1", "tag2" },
                Amount = 50.0f,
                UserId = userId
            },
            new TemplateEntity
            {
                Id = Guid.NewGuid(),
                Name = "Template 2",
                CategoryName = "salary",
                CategoryType = CategoryType.INCOME,
                TagNames = new List<string> { "tag3" },
                Amount = 1500.0f,
                UserId = userId
            }
        });

        await db.SaveChangesAsync();
        var resp = await _client.GetAsync("/smartflow/v1/record-templates");

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task DeleteRecordTemplate_Should_Returns_Ok_And_Remove_RecordTemplate()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.RecordTemplate.Add(new TemplateEntity
        {
            Id = Guid.NewGuid(),
            Name = "Template To Delete",
            CategoryName = "food",
            CategoryType = CategoryType.EXPENSE,
            TagNames = new List<string> { "tag1", "tag2" },
            Amount = 100.0f,
            UserId = userId
        });

        await db.SaveChangesAsync();

        var template = await db.RecordTemplate.FirstOrDefaultAsync(rt => rt.Name == "Template To Delete" && rt.UserId == userId);
        Assert.IsNotNull(template, "RecordTemplate should exist in the database before deletion");

        var DeleteRecordTemplateReq =
        """
        {
            "name": "Template To Delete"
        }
        """;

        var req = new HttpRequestMessage(HttpMethod.Delete, "smartflow/v1/record-template")
        {
            Content = new StringContent(DeleteRecordTemplateReq, Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(req);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var deletedTemplate = await db.RecordTemplate.FirstOrDefaultAsync(rt => rt.Name == "Template To Delete" && rt.UserId == userId);
        Assert.IsNull(deletedTemplate, "RecordTemplate should be deleted from the database");
    }

    [Test]
    public async Task DeleteRecordTemplate_When_TemplateNameIsNotExist_Should_Return_Ok()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var DeleteRecordTemplateReq =
        """
        {
            "name": "Template To Delete"
        }
        """;

        var req = new HttpRequestMessage(HttpMethod.Delete, "smartflow/v1/record-template")
        {
            Content = new StringContent(DeleteRecordTemplateReq, Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(req);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeLogger<RecordTemplateController> fakeLogger = null!;

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            fakeLogger = new FakeLogger<RecordTemplateController>();
            services.AddSingleton<ILogger<RecordTemplateController>>(fakeLogger);

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PostgresDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            var conn = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres_Connection")!;
            services.AddDbContext<PostgresDbContext>(options => options.UseNpgsql(conn));
        });
    }
}