using Application.Common;
using MediatR;

namespace Application.Features.Alerts.Commands.AcknowledgeAlert;

public record AcknowledgeAlertCommand(int Id) : IRequest<Result>;
