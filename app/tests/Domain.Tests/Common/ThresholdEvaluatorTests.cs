using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Domain.Tests.Common;

public class ThresholdEvaluatorTests
{
    private readonly Server _testServer;

    public ThresholdEvaluatorTests()
    {
        _testServer = new Server
        {
            Id = 1,
            Name = "Test Server",
            CpuWarningThreshold = 70,
            CpuCriticalThreshold = 90,
            MemoryWarningThreshold = 80,
            MemoryCriticalThreshold = 95,
            DiskWarningThreshold = 85,
            DiskCriticalThreshold = 98
        };
    }

    [Theory]
    [InlineData(50, false, AlertSeverity.Info)]
    [InlineData(75, true, AlertSeverity.Warning)]
    [InlineData(95, true, AlertSeverity.Critical)]
    public void EvaluateCpu_ShouldReturnCorrectAlertData(double usage, bool expectedShouldAlert, AlertSeverity expectedSeverity)
    {
        // Act
        var result = ThresholdEvaluator.EvaluateCpu(_testServer, usage);

        // Assert
        result.ShouldAlert.Should().Be(expectedShouldAlert);
        if (expectedShouldAlert)
        {
            result.Severity.Should().Be(expectedSeverity);
            result.Message.Should().NotBeEmpty();
        }
    }

    [Theory]
    [InlineData(50, false, AlertSeverity.Info)]
    [InlineData(85, true, AlertSeverity.Warning)]
    [InlineData(97, true, AlertSeverity.Critical)]
    public void EvaluateMemory_ShouldReturnCorrectAlertData(double usage, bool expectedShouldAlert, AlertSeverity expectedSeverity)
    {
        // Act
        var result = ThresholdEvaluator.EvaluateMemory(_testServer, usage);

        // Assert
        result.ShouldAlert.Should().Be(expectedShouldAlert);
        if (expectedShouldAlert)
        {
            result.Severity.Should().Be(expectedSeverity);
            result.Message.Should().NotBeEmpty();
        }
    }

    [Theory]
    [InlineData(50, false, AlertSeverity.Info)]
    [InlineData(90, true, AlertSeverity.Warning)]
    [InlineData(99, true, AlertSeverity.Critical)]
    public void EvaluateDisk_ShouldReturnCorrectAlertData(double usage, bool expectedShouldAlert, AlertSeverity expectedSeverity)
    {
        // Act
        var result = ThresholdEvaluator.EvaluateDisk(_testServer, usage);

        // Assert
        result.ShouldAlert.Should().Be(expectedShouldAlert);
        if (expectedShouldAlert)
        {
            result.Severity.Should().Be(expectedSeverity);
            result.Message.Should().NotBeEmpty();
        }
    }

    [Theory]
    [InlineData(50, 50, 50, ServerStatus.Online)]
    [InlineData(75, 50, 50, ServerStatus.Warning)]
    [InlineData(95, 50, 50, ServerStatus.Critical)]
    [InlineData(50, 85, 50, ServerStatus.Warning)]
    [InlineData(50, 97, 50, ServerStatus.Critical)]
    public void DetermineServerStatus_ShouldReturnCorrectStatus(double cpu, double mem, double disk, ServerStatus expectedStatus)
    {
        // Act
        var result = ThresholdEvaluator.DetermineServerStatus(cpu, mem, disk, _testServer);

        // Assert
        result.Should().Be(expectedStatus);
    }
}
