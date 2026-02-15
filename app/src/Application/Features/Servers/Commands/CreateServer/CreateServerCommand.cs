using Application.Common;
using Application.DTOs.Servers;
using MediatR;

namespace Application.Features.Servers.Commands.CreateServer;

public record CreateServerCommand(
    string Name,
    string HostName,
    string? IpAddress,
    string? Description,
    string? Location,
    string? OperatingSystem,
    double CpuWarningThreshold = 70.0,
    double CpuCriticalThreshold = 90.0,
    double MemoryWarningThreshold = 70.0,
    double MemoryCriticalThreshold = 90.0,
    double DiskWarningThreshold = 80.0,
    double DiskCriticalThreshold = 95.0) : IRequest<Result<ServerDto>>;
