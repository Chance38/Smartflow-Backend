using Testcontainers.PostgreSql;
using DotNet.Testcontainers.Builders;

namespace SmartFlowBackend.Test.Helper;

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
}