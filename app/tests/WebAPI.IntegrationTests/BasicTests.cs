using FluentAssertions;
using Xunit;

namespace WebAPI.IntegrationTests;

public class BasicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public BasicTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act & Assert
        // In CI/Docker, SQL Server and Redis might take a few moments to become ready
        // even after the container has started. We retry for up to 40 seconds (20 retries * 2s).
        HttpResponseMessage response = null!;
        for (int i = 0; i < 20; i++)
        {
            response = await client.GetAsync("/health");
            if (response.IsSuccessStatusCode) break;
            
            // If it's not ready, wait 2 seconds and try again
            await Task.Delay(2000);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"Health check failed after 40s with status {response.StatusCode}. Details: {errorBody}");
        }

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }
}
