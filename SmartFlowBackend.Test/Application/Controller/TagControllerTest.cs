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
using Microsoft.AspNetCore.Http;

namespace SmartFlowBackend.Test.Application.Controller.Tag;

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

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Tag.RemoveRange(db.Tag);
        await db.SaveChangesAsync();
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
        var AddTagReq =
        """
        {
            "tagName": "Test Tag"
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

        var tag = await db.Tag.FirstOrDefaultAsync(c => c.TagName == "Test Tag");
        Assert.IsNotNull(tag, "Tag should be persisted to DB");
    }

    [Test]
    public async Task AddTag_When_The_Same_TagName_is_Exist_Should_Return_BadRequest()
    {
        var AddTagReq =
        """
        {
            "tagName": "Test Tag"
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

        var tag = await db.Tag.FirstOrDefaultAsync(c => c.TagName == "Test Tag");
        Assert.IsNotNull(tag, "Tag should be persisted to DB");

        var response2 = await _client.PostAsync("smartflow/v1/tag", new StringContent(
            AddTagReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response2.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task GetAllTags_Should_Return_Ok()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Tag.AddRange(new List<Domain.Entities.Tag>
        {
            new Domain.Entities.Tag
            {
                TagName = "Test Tag",
                UserId = TestUser.Id
            },
            new Domain.Entities.Tag
            {
                TagName = "Test Tag 2",
                UserId = TestUser.Id
            }
        });

        await db.SaveChangesAsync();

        var t1 = await db.Tag.FirstOrDefaultAsync(c => c.TagName == "Test Tag");
        var t2 = await db.Tag.FirstOrDefaultAsync(c => c.TagName == "Test Tag 2");
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
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Tag.AddRange(new List<Domain.Entities.Tag>
        {
            new Domain.Entities.Tag
            {
                TagName = "Test Tag",
                UserId = TestUser.Id
            }
        });

        await db.SaveChangesAsync();

        var t1 = await db.Tag.FirstOrDefaultAsync(c => c.TagName == "Test Tag");
        Assert.IsNotNull(t1, "Tag should be persisted to DB");

        var deleteReq =
        """
        {
            "tagName": "Test Tag"
        }
        """;

        var req = new HttpRequestMessage(HttpMethod.Delete, "smartflow/v1/tag")
        {
            Content = new StringContent(deleteReq, Encoding.UTF8, "application/json")
        };

        var resp = await _client.SendAsync(req);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var deletedTag = await db.Tag.FirstOrDefaultAsync(c => c.TagName == "Test Tag");
        Assert.IsNull(deletedTag, "Tag should be deleted from DB");
    }

    [Test]
    public async Task DeleteTag_When_Tag_Is_Not_Exit_Should_Return_Ok()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var t1 = await db.Tag.FirstOrDefaultAsync(c => c.TagName == "Test Tag");
        Assert.IsNull(t1, "Tag should not exist in DB");

        var count = await db.Tag.CountAsync();
        Assert.That(count, Is.EqualTo(0), "There should be no tags in DB");

        var deleteReq =
        """
        {
            "tagName": "Test Tag"
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
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.Tag.AddRange(new List<Domain.Entities.Tag>
        {
            new Domain.Entities.Tag
            {
                TagName = "Test Tag",
                UserId = TestUser.Id
            }
        });

        await db.SaveChangesAsync();

        var t1 = await db.Tag.FirstOrDefaultAsync(c => c.TagName == "Test Tag");
        Assert.IsNotNull(t1, "Tag should be persisted to DB");

        var updateReq =
        """
        {
            "oldTagName": "Test Tag",
            "newTagName": "Updated Tag"
        }
        """;

        var resp = await _client.PutAsync("smartflow/v1/tag", new StringContent(
            updateReq,
            Encoding.UTF8,
            "application/json"
        ));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));

        var oldTag = await db.Tag.FirstOrDefaultAsync(c => c.TagName == "Test Tag");
        Assert.IsNull(oldTag, "Old tag name should not exist in DB");

        var updatedTag = await db.Tag.FirstOrDefaultAsync(c => c.TagName == "Updated Tag");
        Assert.IsNotNull(updatedTag, "Tag name should be updated in DB");
    }

    [Test]
    public async Task UpdateTag_When_Tag_Is_Not_Exist_Should_Return_BadRequest()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        var t1 = await db.Tag.FirstOrDefaultAsync(c => c.TagName == "Test Tag");
        Assert.IsNull(t1, "Tag should not exist in DB");

        var count = await db.Tag.CountAsync();
        Assert.That(count, Is.EqualTo(0), "There should be no tags in DB");

        var updateReq =
        """
        {
            "oldTagName": "Test Tag",
            "newTagName": "Updated Tag"
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