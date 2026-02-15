using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Domain.Tests.Entities;

public class AlertTests
{
    [Fact]
    public void CanCreateAlert_ShouldReturnTrue_WhenNoLastAlert()
    {
        // Act
        var result = Alert.CanCreateAlert(null, TimeSpan.FromMinutes(5));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanCreateAlert_ShouldReturnFalse_WhenWithinAntiSpamWindow()
    {
        // Arrange
        var lastAlert = DateTime.UtcNow.AddMinutes(-2);
        var window = TimeSpan.FromMinutes(5);

        // Act
        var result = Alert.CanCreateAlert(lastAlert, window);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanCreateAlert_ShouldReturnTrue_WhenOutsideAntiSpamWindow()
    {
        // Arrange
        var lastAlert = DateTime.UtcNow.AddMinutes(-10);
        var window = TimeSpan.FromMinutes(5);

        // Act
        var result = Alert.CanCreateAlert(lastAlert, window);

        // Assert
        result.Should().BeTrue();
    }
}
