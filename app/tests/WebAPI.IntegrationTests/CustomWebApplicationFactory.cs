using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;
using Testcontainers.Redis;
using Xunit;

namespace WebAPI.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private MsSqlContainer? _dbContainer;
    private RedisContainer? _redisContainer;
    
    private bool _useContainers = false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // ───────────────────────────────────────────────────────────────────
        // FORCE DEVELOPMENT ENVIRONMENT
        // This ensures the seeding/migration block in Program.cs executes
        // ───────────────────────────────────────────────────────────────────
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            if (_dbContainer != null && _redisContainer != null)
            {
                var dbConn = _dbContainer.GetConnectionString();
                if (!dbConn.Contains("TrustServerCertificate")) dbConn += ";TrustServerCertificate=True";

                var overrides = new Dictionary<string, string?>
                {
                    { "ConnectionStrings:DefaultConnection", dbConn },
                    { "ConnectionStrings:Redis", _redisContainer.GetConnectionString() }
                };

                config.AddInMemoryCollection(overrides);
            }
        });

        builder.ConfigureServices(services =>
        {
            // Optional: override services if needed
        });
    }

    public async Task InitializeAsync()
    {
        try
        {
            _dbContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .Build();

            _redisContainer = new RedisBuilder()
                .WithImage("redis:7-alpine")
                .Build();

            // Start containers
            await Task.WhenAll(_dbContainer.StartAsync(), _redisContainer.StartAsync());

            // ───────────────────────────────────────────────────────────────────
            // CRITICAL FIX: Set Environment Variables
            // This is the MOST reliable way to override appsettings.json in .NET
            // ───────────────────────────────────────────────────────────────────
            var dbConn = _dbContainer.GetConnectionString();
            if (!dbConn.Contains("TrustServerCertificate")) dbConn += ";TrustServerCertificate=True";
            
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", dbConn);
            Environment.SetEnvironmentVariable("ConnectionStrings__Redis", _redisContainer.GetConnectionString());
            
            _useContainers = true;
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine("[INTEGRATION TEST] Docker Containers Started Successfully");
            Console.WriteLine($"[SQL] {dbConn}");
            Console.WriteLine("----------------------------------------------------------------------");
        }
        catch (Exception ex)
        {
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine("ERROR: Failed to start Docker containers.");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("----------------------------------------------------------------------");
            _useContainers = false;
        }
    }

    public new async Task DisposeAsync()
    {
        if (_dbContainer != null)
            await _dbContainer.StopAsync();
            
        if (_redisContainer != null)
            await _redisContainer.StopAsync();
    }
}
