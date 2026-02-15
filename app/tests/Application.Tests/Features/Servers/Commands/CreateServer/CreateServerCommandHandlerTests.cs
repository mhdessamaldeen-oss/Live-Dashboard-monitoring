using Application.Common.Mappings;
using Application.DTOs.Servers;
using Application.Features.Servers.Commands.CreateServer;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Application.Tests.Features.Servers.Commands.CreateServer;

public class CreateServerCommandHandlerTests
{
    private readonly Mock<IServerRepository> _serverRepositoryMock;
    private readonly IMapper _mapper;
    private readonly CreateServerCommandHandler _handler;

    public CreateServerCommandHandlerTests()
    {
        _serverRepositoryMock = new Mock<IServerRepository>();
        
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        
        _handler = new CreateServerCommandHandler(_serverRepositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldCreateServer_WhenNameIsUnique()
    {
        // Arrange
        var command = new CreateServerCommand(
            Name: "New Server",
            HostName: "HOSTNAME",
            IpAddress: "127.0.0.1",
            Description: "Desc",
            Location: "Loc",
            OperatingSystem: "OS"
        );

        _serverRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Server, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be(command.Name);
        _serverRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Server>(), It.IsAny<CancellationToken>()), Times.Once);
        _serverRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNameAlreadyExists()
    {
        // Arrange
        var command = new CreateServerCommand(
            Name: "Existing Server",
            HostName: "HOSTNAME",
            IpAddress: "127.0.0.1",
            Description: "Desc",
            Location: "Loc",
            OperatingSystem: "OS"
        );

        _serverRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Server, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("A server with this name already exists.");
        _serverRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Server>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
