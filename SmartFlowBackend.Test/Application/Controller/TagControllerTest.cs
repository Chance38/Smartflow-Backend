using System.Net;
using System.Text;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Logging;

using Application.Controller;
using Infrastructure.Persistence;

using Test.Helper;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace SmartFlowBackend.Test.Application.Controller.Tag;

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
    public async Task AddTag_Should_Return_Ok()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var AddTagReq =
        """
        {
            "tag": {
                "name": "Test Tag"
            }
        }
        """;

        var response = await _client.PostAsync("smartflow/v1/tag", new StringContent(
            AddTagReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var tag = await db.Tag.FirstOrDefaultAsync(t => t.Name == "Test Tag" && t.UserId == userId);
        Assert.IsNotNull(tag, "Tag should be persisted to DB");
    }

    [Test]
    public async Task AddTag_When_The_Same_TagName_is_Exist_Should_Return_BadRequest()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Tag.AddRange(new List<Domain.Entity.Tag>
        {
            new Domain.Entity.Tag
            {
                Name = "Test Tag",
                UserId = userId
            }
        });

        db.SaveChanges();

        var tag = await db.Tag.FirstOrDefaultAsync(t => t.Name == "Test Tag" && t.UserId == userId);
        Assert.IsNotNull(tag, "Tag should be persisted to DB");

        var AddTagReq =
        """
        {
            "tag": {
                "name": "Test Tag"
            }
        }
        """;

        var resp = await _client.PostAsync("smartflow/v1/tag", new StringContent(
            AddTagReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task GetAllTags_Should_Return_Ok()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Tag.AddRange(new List<Domain.Entity.Tag>
        {
            new Domain.Entity.Tag
            {
                Name = "Test Tag",
                UserId = userId
            },
            new Domain.Entity.Tag
            {
                Name = "Test Tag 2",
                UserId = userId
            }
        });

        await db.SaveChangesAsync();

        var t1 = await db.Tag.FirstOrDefaultAsync(t => t.Name == "Test Tag" && t.UserId == userId);
        var t2 = await db.Tag.FirstOrDefaultAsync(t => t.Name == "Test Tag 2" && t.UserId == userId);
        Assert.IsNotNull(t1, "Tag1 should be persisted to DB");
        Assert.IsNotNull(t2, "Tag2 should be persisted to DB");

        var resp = await _client.GetAsync("smartflow/v1/tags");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));

        Assert.That(content, Does.Contain("Test Tag"));
        Assert.That(content, Does.Contain("Test Tag 2"));

        // 不能使用固定的 Assert 測法，因為回傳的順序是隨機的
        // var jsonResp = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
        // var actualTags = jsonResp["tags"];
        // Assert.That(actualTags[0].GetProperty("tagName").GetString(), Is.EqualTo("Test Tag"));
        // Assert.That(actualTags[1].GetProperty("tagName").GetString(), Is.EqualTo("Test Tag 2"));
    }

    [Test]
    public async Task DeleteTag_Should_Return_Ok()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Tag.AddRange(new List<Domain.Entity.Tag>
        {
            new Domain.Entity.Tag
            {
                Name = "Test Tag",
                UserId = userId
            }
        });

        await db.SaveChangesAsync();

        var t1 = await db.Tag.FirstOrDefaultAsync(c => c.Name == "Test Tag");
        Assert.IsNotNull(t1, "Tag should be persisted to DB");

        var deleteReq =
        """
        {
            "tag": {
                "name": "Test Tag"
            }
        }
        """;

        var req = new HttpRequestMessage(HttpMethod.Delete, "smartflow/v1/tag")
        {
            Content = new StringContent(deleteReq, Encoding.UTF8, "application/json")
        };

        var resp = await _client.SendAsync(req);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var deletedTag = await db.Tag.FirstOrDefaultAsync(t => t.Name == "Test Tag" && t.UserId == userId);
        Assert.IsNull(deletedTag, "Tag should be deleted from DB");
    }

    [Test]
    public async Task DeleteTag_When_Tag_Is_Not_Exit_Should_Return_Ok()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var count = await db.Tag.CountAsync(t => t.UserId == userId);
        Assert.That(count, Is.EqualTo(0), "There should be no tags in DB");

        var deleteReq =
        """
        {
            "tag": {
                "name": "Test Tag"
            }
        }
        """;

        var req = new HttpRequestMessage(HttpMethod.Delete, "smartflow/v1/tag")
        {
            Content = new StringContent(deleteReq, Encoding.UTF8, "application/json")
        };

        var resp = await _client.SendAsync(req);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task UpdateTag_Should_Return_Ok()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Tag.AddRange(new List<Domain.Entity.Tag>
        {
            new Domain.Entity.Tag
            {
                Name = "Test Tag",
                UserId = userId
            }
        });

        await db.SaveChangesAsync();

        var t1 = await db.Tag.FirstOrDefaultAsync(t => t.Name == "Test Tag" && t.UserId == userId);
        Assert.IsNotNull(t1, "Tag should be persisted to DB");

        var updateReq =
        """
        {
            "oldTag": {
                "name": "Test Tag"
            },
            "newTag": {
                "name": "Updated Tag"
            }
        }
        """;

        var resp = await _client.PutAsync("smartflow/v1/tag", new StringContent(
            updateReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));

        var oldTag = await db.Tag.FirstOrDefaultAsync(t => t.Name == "Test Tag" && t.UserId == userId);
        Assert.IsNull(oldTag, "Old tag name should not exist in DB");

        var updatedTag = await db.Tag.FirstOrDefaultAsync(t => t.Name == "Updated Tag" && t.UserId == userId);
        Assert.IsNotNull(updatedTag, "Tag name should be updated in DB");
    }

    [Test]
    public async Task UpdateTag_When_Tag_Is_Not_Exist_Should_Return_BadRequest()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var count = await db.Tag.CountAsync(t => t.UserId == userId);
        Assert.That(count, Is.EqualTo(0), "There should be no tags in DB");

        var updateReq =
        """
        {
            "oldTag": {
                "name": "Test Tag"
            },
            "newTag": {
                "name": "Updated Tag"
            }
        }
        """;

        var resp = await _client.PutAsync("smartflow/v1/tag", new StringContent(
            updateReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
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