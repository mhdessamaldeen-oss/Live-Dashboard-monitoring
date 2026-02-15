using Application.Common.Mappings;
using Application.DTOs.Alerts;
using Application.DTOs.Metrics;
using Application.Features.Metrics.Commands.CollectMetrics;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.Features.Metrics.Commands.CollectMetrics;

public class CollectMetricsCommandHandlerTests
{
    private readonly Mock<IServerRepository> _serverRepositoryMock;
    private readonly Mock<IAlertRepository> _alertRepositoryMock;
    private readonly Mock<IMetricRepository> _metricRepositoryMock;
    private readonly Mock<IMetricsProvider> _metricsProviderMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<CollectMetricsCommandHandler>> _loggerMock;
    private readonly Mock<IDiskRepository> _diskRepositoryMock;
    private readonly IMapper _mapper;
    private readonly CollectMetricsCommandHandler _handler;

    public CollectMetricsCommandHandlerTests()
    {
        _serverRepositoryMock = new Mock<IServerRepository>();
        _alertRepositoryMock = new Mock<IAlertRepository>();
        _metricRepositoryMock = new Mock<IMetricRepository>();
        _metricsProviderMock = new Mock<IMetricsProvider>();
        _notificationServiceMock = new Mock<INotificationService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<CollectMetricsCommandHandler>>();

        _diskRepositoryMock = new Mock<IDiskRepository>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _handler = new CollectMetricsCommandHandler(
            _serverRepositoryMock.Object,
            _alertRepositoryMock.Object,
            _metricRepositoryMock.Object,
            _diskRepositoryMock.Object,
            _metricsProviderMock.Object,
            _notificationServiceMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldCollectMetricsAndNotify_WhenServerIsValid()
    {
        // Arrange
        var serverId = 1;
        var server = new Server 
        { 
            Id = serverId, 
            IsActive = true, 
            CpuWarningThreshold = 70, 
            CpuCriticalThreshold = 90,
            MemoryWarningThreshold = 80,
            MemoryCriticalThreshold = 95,
            DiskWarningThreshold = 85,
            DiskCriticalThreshold = 98
        };
        
        var metricDto = new MetricDto { CpuUsagePercent = 50, MemoryUsagePercent = 60, DiskUsagePercent = 40 };

        _serverRepositoryMock.Setup(x => x.GetActiveServerByIdAsync(serverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(server);

        _metricsProviderMock.Setup(x => x.CollectMetricsAsync(serverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(metricDto);

        _alertRepositoryMock.Setup(x => x.GetLastActiveAlertAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Alert?)null);

        // Act
        var result = await _handler.Handle(new CollectMetricsCommand(serverId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _metricRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync(It.Is<string>(s => s.Contains(serverId.ToString())), It.IsAny<LatestMetricsDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(x => x.SendMetricUpdateAsync(serverId, It.IsAny<LatestMetricsDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateAlert_WhenThresholdBreached()
    {
        // Arrange
        var serverId = 1;
        var server = new Server 
        { 
            Id = serverId, 
            IsActive = true, 
            CpuWarningThreshold = 70, 
            CpuCriticalThreshold = 90,
            MemoryWarningThreshold = 80,
            MemoryCriticalThreshold = 95,
            DiskWarningThreshold = 85,
            DiskCriticalThreshold = 98
        };
        
        // Breach CPU threshold
        var metricDto = new MetricDto { CpuUsagePercent = 95, MemoryUsagePercent = 60, DiskUsagePercent = 40 };

        _serverRepositoryMock.Setup(x => x.GetActiveServerByIdAsync(serverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(server);

        _metricsProviderMock.Setup(x => x.CollectMetricsAsync(serverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(metricDto);

        _alertRepositoryMock.Setup(x => x.GetLastActiveAlertAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Alert?)null);

        // Act
        var result = await _handler.Handle(new CollectMetricsCommand(serverId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _alertRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _notificationServiceMock.Verify(x => x.SendAlertTriggeredAsync(It.IsAny<AlertDto>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
