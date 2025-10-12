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

namespace Test.Application.Controller.Summary;

public class SummaryControllerTest
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

    [Test]
    public async Task GetMonthSummaries_When_Period_Is_1_Should_Return_Ok()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.MonthlySummary.Add(new Domain.Entity.MonthlySummary
        {
            UserId = userId,
            Year = DateTime.UtcNow.Year,
            Month = DateTime.UtcNow.Month,
            Expense = 50.75f,
            Income = 0.00f
        });

        await db.SaveChangesAsync();

        var resp = await _client.GetAsync("/smartflow/v1/month-summary?period=1");
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
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var resp = await _client.GetAsync("/smartflow/v1/month-summary?period=1");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(resp.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task GetMonthRecords_When_Period_Is_6_Should_Return_Ok()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.MonthlySummary.AddRange(new List<Domain.Entity.MonthlySummary>
        {
            new Domain.Entity.MonthlySummary
            {
                UserId = userId,
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.Month,
                Expense = 50.75f,
                Income = 0.00f
            },
            new Domain.Entity.MonthlySummary
            {
                UserId = userId,
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.AddMonths(-1).Month,
                Expense = 100.00f,
                Income = 200.00f
            },
            new Domain.Entity.MonthlySummary
            {
                UserId = userId,
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.AddMonths(-3).Month,
                Expense = 300.00f,
                Income = 400.00f
            }
        });

        await db.SaveChangesAsync();

        var resp = await _client.GetAsync("/smartflow/v1/month-summary?period=6");
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
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var resp = await _client.GetAsync("/smartflow/v1/month-summary?period=3");

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(resp.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task GetMonthRecords_When_Period_Is_Not_Query_Should_Return_Ok_With_All_Month_Records()
    {
        var userId = Guid.NewGuid();
        var token = TestHelper.CreateMockAccessToken(userId);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        db.MonthlySummary.AddRange(new List<Domain.Entity.MonthlySummary>
        {
            new Domain.Entity.MonthlySummary
            {
                UserId = userId,
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.Month,
                Expense = 50.75f,
                Income = 0.00f
            },
            new Domain.Entity.MonthlySummary
            {
                UserId = userId,
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.AddMonths(-1).Month,
                Expense = 100.00f,
                Income = 200.00f
            },
            new Domain.Entity.MonthlySummary
            {
                UserId = userId,
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.AddMonths(-3).Month,
                Expense = 300.00f,
                Income = 400.00f
            }
        });

        await db.SaveChangesAsync();

        var resp = await _client.GetAsync("/smartflow/v1/month-summary");
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