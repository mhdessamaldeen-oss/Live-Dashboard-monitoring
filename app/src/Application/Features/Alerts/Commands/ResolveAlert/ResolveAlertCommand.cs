using Application.Common;
using Application.DTOs.Alerts;
using MediatR;

namespace Application.Features.Alerts.Commands.ResolveAlert;

public record ResolveAlertCommand(int Id, string? Resolution) : IRequest<Result<AlertDto>>;
