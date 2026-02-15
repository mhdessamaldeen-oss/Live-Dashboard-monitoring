using Application.Common;
using Application.DTOs.Servers;
using MediatR;

namespace Application.Features.Servers.Commands.UpdateServer;

public record UpdateServerCommand(
    int Id,
    string Name,
    string HostName,
    string? IpAddress,
    string? Description,
    string? Location,
    string? OperatingSystem,
    bool IsActive,
    double CpuWarningThreshold,
    double CpuCriticalThreshold,
    double MemoryWarningThreshold,
    double MemoryCriticalThreshold,
    double DiskWarningThreshold,
    double DiskCriticalThreshold) : IRequest<Result<ServerDto>>;
