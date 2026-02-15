using Application.Common.Mappings;
using Application.DTOs.Alerts;
using Application.Features.Alerts.Commands.ResolveAlert;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.Features.Alerts.Commands.ResolveAlert;

public class ResolveAlertCommandHandlerTests
{
    private readonly Mock<IAlertRepository> _alertRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly IMapper _mapper;
    private readonly ResolveAlertCommandHandler _handler;

    public ResolveAlertCommandHandlerTests()
    {
        _alertRepositoryMock = new Mock<IAlertRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _notificationServiceMock = new Mock<INotificationService>();
        
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        
        _handler = new ResolveAlertCommandHandler(
            _alertRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldResolveAlert_WhenAlertExistsAndActive()
    {
        // Arrange
        var alertId = 1;
        var alert = new Alert { Id = alertId, Status = AlertStatus.Active, Title = "Test Alert" };
        var userId = 10;

        _alertRepositoryMock.Setup(x => x.GetWithServerByIdAsync(alertId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alert);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        // Act
        var result = await _handler.Handle(new ResolveAlertCommand(alertId, "Fixed central issue"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        alert.Status.Should().Be(AlertStatus.Resolved);
        alert.ResolvedByUserId.Should().Be(userId);
        _alertRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(x => x.SendAlertResolvedAsync(It.IsAny<AlertDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAlertNotFound()
    {
        // Arrange
        _alertRepositoryMock.Setup(x => x.GetWithServerByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Alert?)null);

        // Act
        var result = await _handler.Handle(new ResolveAlertCommand(999, "Resolution"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Alert not found.");
    }
}
