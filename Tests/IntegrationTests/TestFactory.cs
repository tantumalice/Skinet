using Core.Entites.Identity;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stripe;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace IntegrationTests;

public class TestFactory<T> : WebApplicationFactory<T>, IAsyncLifetime where T : class
{
    private readonly PostgreSqlContainer _storeContainer;
    private readonly PostgreSqlContainer _identityContainer;
    private readonly RedisContainer _redisContainer;

    public TestFactory()
    {
        _storeContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("TestDatabase")
            .WithName("TestPostgreStore")
            .WithUsername("TestUser")
            .WithPassword("TestPassword")
            .WithHostname("localhost")
            .WithCleanUp(true)
            .Build();

        _identityContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("IdentityDatabase")
            .WithName("TestPostgreIdentity")
            .WithUsername("TestUser")
            .WithPassword("TestPassword")
            .WithHostname("localhost")
            .WithCleanUp(true)
            .Build();

        _redisContainer = new RedisBuilder()
            .WithCleanUp(true)
            .WithImage("redis:7.0.8")
            .WithName("TestRedis")
            .WithHostname("localhost")
            .WithPortBinding("6379")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove DbContext
            services.RemoveDbContext<StoreContext>();
            services.RemoveDbContext<AppIdentityDbContext>();

            // Add DB context pointing to test container
            services.AddDbContext<StoreContext>(options =>
            {
                options.UseNpgsql(_storeContainer.GetConnectionString());
            });
            services.AddDbContext<AppIdentityDbContext>(options =>
            {
                options.UseNpgsql(_identityContainer.GetConnectionString());
            });

            // Seed
            services.SeedStoreDb();
            services.SeedIdentityDb();
        });
    }

    public async Task InitializeAsync()
    {
        await _storeContainer.StartAsync();
        await _identityContainer.StartAsync();
        await _redisContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _storeContainer.DisposeAsync();
        await _identityContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
    }
}
