using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using DotNet.Testcontainers.Builders;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Test.Helper;

public class TestHelper
{
    public static PostgreSqlContainer CreatePostgreSqlContainer()
    {
        var postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("password")
            .WithImage("postgres:15-alpine")
            .WithCleanUp(true)
            .WithPortBinding(5432, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();

        return postgresContainer;
    }

    public static RabbitMqContainer CreateRabbitMQContainer()
    {
        var rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true)
            .WithCleanUp(true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .Build();

        return rabbitMqContainer;
    }

    public static string CreateMockAccessToken(Guid userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SmartechAFk9Jlh9qTPXWLJxGjsoglsigaoGJIKey"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "SmartechIssuer",
            audience: "SmartechAudience",
            claims: claims,
            expires: DateTime.Now.AddSeconds(10),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}