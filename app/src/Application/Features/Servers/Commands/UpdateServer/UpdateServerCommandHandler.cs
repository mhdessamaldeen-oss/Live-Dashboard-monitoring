using Application.Common;
using Application.DTOs.Servers;
using Application.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Features.Servers.Commands.UpdateServer;

public class UpdateServerCommandHandler : IRequestHandler<UpdateServerCommand, Result<ServerDto>>
{
    private readonly IServerRepository _serverRepository;
    private readonly IMapper _mapper;

    public UpdateServerCommandHandler(IServerRepository serverRepository, IMapper mapper)
    {
        _serverRepository = serverRepository;
        _mapper = mapper;
    }

    public async Task<Result<ServerDto>> Handle(UpdateServerCommand request, CancellationToken cancellationToken)
    {
        var server = await _serverRepository.GetByIdAsync(request.Id, cancellationToken);

        if (server == null)
        {
            return Result<ServerDto>.Failure("Server not found.");
        }

        // Check if name is taken by another server
        if (await _serverRepository.AnyAsync(s => s.Name == request.Name && s.Id != request.Id, cancellationToken))
        {
            return Result<ServerDto>.Failure("A server with this name already exists.");
        }

        server.Name = request.Name;
        server.HostName = request.HostName;
        server.IpAddress = request.IpAddress;
        server.Description = request.Description;
        server.Location = request.Location;
        server.OperatingSystem = request.OperatingSystem;
        server.IsActive = request.IsActive;
        server.CpuWarningThreshold = request.CpuWarningThreshold;
        server.CpuCriticalThreshold = request.CpuCriticalThreshold;
        server.MemoryWarningThreshold = request.MemoryWarningThreshold;
        server.MemoryCriticalThreshold = request.MemoryCriticalThreshold;
        server.DiskWarningThreshold = request.DiskWarningThreshold;
        server.DiskCriticalThreshold = request.DiskCriticalThreshold;

        _serverRepository.Update(server);
        await _serverRepository.SaveChangesAsync(cancellationToken);

        return Result<ServerDto>.Success(_mapper.Map<ServerDto>(server));
    }
}
