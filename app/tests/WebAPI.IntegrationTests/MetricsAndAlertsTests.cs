using System.Net.Http.Json;
using Application.Common;
using Application.DTOs.Auth;
using Application.DTOs.Metrics;
using Application.DTOs.Alerts;
using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Xunit;

namespace WebAPI.IntegrationTests;

public class MetricsAndAlertsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public MetricsAndAlertsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> GetAuthorizedClientAsync()
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("admin@demo.com", "Admin123!"));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Result<AuthResponse>>();
        
        loginResult.Should().NotBeNull();
        loginResult!.IsSuccess.Should().BeTrue($"Login failed with error: {string.Join(", ", loginResult.Errors)}");

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Data!.Token);
        return client;
    }

    [Fact]
    public async Task GetLatestMetrics_ShouldReturnData()
    {
        // Arrange
        var client = await GetAuthorizedClientAsync();
        var serverId = 1;

        // Seed mock metric into cache so the handler has data to return
        using (var scope = _factory.Services.CreateScope())
        {
            var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
            var mockMetric = new LatestMetricsDto(new MetricDto 
            { 
                ServerId = serverId, 
                CpuUsagePercent = 25, 
                MemoryUsagePercent = 40, 
                DiskUsagePercent = 10,
                Status = "Online", 
                Timestamp = DateTime.UtcNow 
            });
            await cache.SetAsync($"server:{serverId}:latest", mockMetric);
        }

        // Act
        var response = await client.GetAsync($"/api/v1/servers/{serverId}/metrics/latest");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Result<LatestMetricsDto>>();
        result!.IsSuccess.Should().BeTrue();
        result.Data!.Metric!.ServerId.Should().Be(serverId);
    }

    [Fact]
    public async Task GetAlerts_ShouldReturnPagedResult()
    {
        // Arrange
        var client = await GetAuthorizedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/alerts?pageSize=5");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<AlertDto>>();
        result!.Items.Should().NotBeNull();
    }
}
