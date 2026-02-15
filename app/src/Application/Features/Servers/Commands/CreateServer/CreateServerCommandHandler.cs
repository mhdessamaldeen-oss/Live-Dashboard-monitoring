using Application.Common;
using Application.DTOs.Servers;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Servers.Commands.CreateServer;

public class CreateServerCommandHandler : IRequestHandler<CreateServerCommand, Result<ServerDto>>
{
    private readonly IServerRepository _serverRepository;
    private readonly IMapper _mapper;

    public CreateServerCommandHandler(IServerRepository serverRepository, IMapper mapper)
    {
        _serverRepository = serverRepository;
        _mapper = mapper;
    }

    public async Task<Result<ServerDto>> Handle(CreateServerCommand request, CancellationToken cancellationToken)
    {
        if (await _serverRepository.AnyAsync(s => s.Name == request.Name, cancellationToken))
        {
            return Result<ServerDto>.Failure("A server with this name already exists.");
        }

        var server = _mapper.Map<Server>(request);
        server.Status = Domain.Enums.ServerStatus.Unknown;
        server.IsActive = true;

        await _serverRepository.AddAsync(server, cancellationToken);
        await _serverRepository.SaveChangesAsync(cancellationToken);

        return Result<ServerDto>.Success(_mapper.Map<ServerDto>(server));
    }
}
