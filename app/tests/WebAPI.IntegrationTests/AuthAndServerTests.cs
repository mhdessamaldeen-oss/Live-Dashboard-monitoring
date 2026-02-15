using System.Net.Http.Json;
using Application.Common;
using Application.DTOs.Auth;
using Application.DTOs.Servers;
using FluentAssertions;
using Xunit;

namespace WebAPI.IntegrationTests;

public class AuthAndServerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthAndServerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new LoginRequest("admin@demo.com", "Admin123!");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponse>>();
        
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue($"Login failed: {string.Join(", ", result.Errors)}");
        result.Data!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Admin_ShouldBeAbleToCreateServer()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // 1. Login to get token
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("admin@demo.com", "Admin123!"));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Result<AuthResponse>>();
        
        loginResult.Should().NotBeNull();
        loginResult!.IsSuccess.Should().BeTrue("Admin login failed during test setup");
        
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Data!.Token);

        var createCommand = new
        {
            Name = "Integration Test Server",
            HostName = "IT-SRV",
            IpAddress = "10.0.0.1",
            Description = "Created via test",
            Location = "Test DC",
            OperatingSystem = "Ubuntu 22.04"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/servers", createCommand);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Result<ServerDto>>();
        result!.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Integration Test Server");
    }

    [Fact]
    public async Task UnauthorizedUser_ShouldNotBeAbleToDeleteServer()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // 1. Login as standard user
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("user@demo.com", "User123!"));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Result<AuthResponse>>();
        
        loginResult.Should().NotBeNull();
        loginResult!.IsSuccess.Should().BeTrue("User login failed during test setup");
        
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Data!.Token);

        // Act
        var response = await client.DeleteAsync("/api/v1/servers/1");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }
}
