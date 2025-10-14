using System.Net;
using System.Text;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using Middleware;
using Domain.Entity;
using CategoryEntity = Domain.Entity.Category;
using Infrastructure.Persistence;

using Test.Helper;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Test.Middleware;

public class ServiceMiddlewareTest
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
    public async Task GetBalance_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("smartflow/v1/balance");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [TestCase("SmartechIssuer", "SmartechAudience", -10)]
    [TestCase("InvalidToken", "SmartechAudience", 10)]
    [TestCase("SmartechIssuer", "InvalidAudience", 10)]
    public async Task GetBalance_WithInvalidToken_ShouldReturnUnauthorized(string issuer, string audience, int expiration)
    {
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SmartechAFk9Jlh9qTPXWLJxGjsoglsigaoGJIKey"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddSeconds(expiration),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");

        var response = await _client.GetAsync("smartflow/v1/balance");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetBalance_WithInvalidToken_ShouldReturnUnauthorized()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SmartechAFk9Jlh9qTPXWLJxGjsoglsigaoGJIKey"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "SmartechIssuer",
            audience: "SmartechAudience",
            expires: DateTime.Now.AddSeconds(10),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");

        var response = await _client.GetAsync("smartflow/v1/balance");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeLogger<ServiceMiddleware> fakeLogger = null!;

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            fakeLogger = new FakeLogger<ServiceMiddleware>();
            services.AddSingleton<ILogger<ServiceMiddleware>>(fakeLogger);

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PostgresDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            var conn = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres_Connection")!;
            services.AddDbContext<PostgresDbContext>(options => options.UseNpgsql(conn));
        });
    }
}