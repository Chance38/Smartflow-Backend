using System.Text;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

using Domain.Subscriber;
using Infrastructure.Persistence;

using Test.Helper;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Test.Domain.Subscriber.User;

public class UserRegisterSubscriberTest
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
    public async Task UserRegisterSubscriber_Should_Handle_UserRegistered_Event()
    {
        var userId = Guid.NewGuid();
        var message = new { UserId = userId };
        var payload = JsonConvert.SerializeObject(message);

        var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
        var rabbitMqPort = Environment.GetEnvironmentVariable("RABBITMQ_PORT")!;
        var rabbitMqUser = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
        var rabbitMqPass = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqHost,
            Port = int.Parse(rabbitMqPort),
            UserName = rabbitMqUser,
            Password = rabbitMqPass
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        var body = Encoding.UTF8.GetBytes(payload);

        channel.BasicPublish(exchange: "IDENTITY",
            routingKey: "REGISTER",
            basicProperties: null,
            body: body);

        await Task.Delay(1000);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();
        var categories = await dbContext.Category.Where(c => c.UserId == userId).ToListAsync();
        var balance = await dbContext.Balance.SingleOrDefaultAsync(b => b.UserId == userId);

        Assert.That(categories, Is.Not.Null);
        Assert.That(categories.Count, Is.EqualTo(5));
        Assert.That(balance, Is.Not.Null);
        Assert.That(balance.Amount, Is.EqualTo(0));
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeLogger<UserRegisterSubscriber> fakeLogger = null!;

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            fakeLogger = new FakeLogger<UserRegisterSubscriber>();
            services.AddSingleton<ILogger<UserRegisterSubscriber>>(fakeLogger);

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PostgresDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            var conn = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres_Connection")!;
            services.AddDbContext<PostgresDbContext>(options => options.UseNpgsql(conn));
        });
    }
}