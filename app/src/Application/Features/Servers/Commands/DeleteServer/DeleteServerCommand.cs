using Application.Common;
using MediatR;

namespace Application.Features.Servers.Commands.DeleteServer;

public record DeleteServerCommand(int Id) : IRequest<Result>;
