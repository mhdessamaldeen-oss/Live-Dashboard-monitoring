using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Servers.Commands.DeleteServer;

public class DeleteServerCommandHandler : IRequestHandler<DeleteServerCommand, Result>
{
    private readonly IServerRepository _serverRepository;

    public DeleteServerCommandHandler(IServerRepository serverRepository)
    {
        _serverRepository = serverRepository;
    }

    public async Task<Result> Handle(DeleteServerCommand request, CancellationToken cancellationToken)
    {
        var server = await _serverRepository.GetByIdAsync(request.Id, cancellationToken);

        if (server == null)
        {
            return Result.Failure("Server not found.");
        }

        await _serverRepository.DeleteWithRelatedAsync(request.Id, cancellationToken);
        
        return Result.Success();
    }
}
