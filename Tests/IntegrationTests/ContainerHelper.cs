using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace IntegrationTests;

public static class ContainerHelper
{
    public static async Task<PostgreSqlContainer> InitDatabaseContainer()
    {
        var container = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("TestDatabase")
            //.WithDatabase("IdentityDatabase")
            .WithName("TestPostgre")
            .WithUsername("TestUser")
            .WithPassword("TestPassword")
            .WithHostname("localhost")
            .WithPortBinding("5455")
            .WithExposedPort("5432")
            .WithCleanUp(true)
            .Build();

        return container;
    }

    public static async Task<RedisContainer> InitRedisContainer()
    {
        var container = new RedisBuilder()
            .WithCleanUp(true)
            .WithImage("redis:7.0.8")
            .WithName("TestRedis")
            .WithHostname("localhost")
            .WithPortBinding("6379")
            .Build();

        return container;
    }
}
